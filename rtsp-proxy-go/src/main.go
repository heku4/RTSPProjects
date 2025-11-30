package main

import (
	b64 "encoding/base64"
	"fmt"
	"io"
	"log"
	"net"
	"os"
	"strconv"
	"strings"
)

const (
	ConnHost                 = "localhost"
	ConnPort                 = "18080"
	ConnType                 = "tcp"
	BufferSize               = 2048
	ServerPort0              = 28080
	ServerPort1              = 28081
	SuccessResponse          = "RTSP/1.0 200 OK"
	NotFoundResponse         = "RTSP/1.0 404 Not Found"
	DescribeMethodSDPHeaders = "Content-Type: application/sdp\r\nv=0\r\ns=RTSP server on Go\r\ni=RTSPServer\r\nm=video 0 RTP/AVP 26\r\na=type:broadcast\r\na=control:track0\r\na=framerate:25.000000\r\n"
)

const (
	OPTIONS  = "options"
	DESCRIBE = "describe"
	SETUP    = "setup"
	PLAY     = "play"
	PAUSE    = "pause"
	TEARDOWN = "teardown"
)

type rtspRequest struct {
	reqType         string
	originalAddress string
	endPoint        string
	receiverAddress string
	protocolType    string
	seqNum          int
	seqStr          string
	authHeaderData  string
	additionalData  string
	transport       string
}

var sessionNumber = 0

func handleIncomingRequest(conn net.Conn, buffer []byte) {
	reqData := string(buffer)
	parsedRequest := parseRequest(reqData)
	go sendResponse(conn, parsedRequest)
}

func parseRequest(data string) rtspRequest {
	log.Printf(data)

	requestByParts := strings.Split(data, "\r\n")

	mainRequestData := strings.Split(requestByParts[0], " ")
	addressData := strings.Split(mainRequestData[1], "/")

	urlDecoded, _ := b64.URLEncoding.DecodeString(addressData[len(addressData)-1])

	seqStr := "CSeq: 0"
	seqNum := 0
	authData := ""
	additionalData := ""
	transportData := ""

	for i := 1; i < len(requestByParts); i++ {
		if strings.Contains(requestByParts[i], "CSeq:") {
			seqStr = requestByParts[i]

			seqStrByParts := strings.Split(requestByParts[i], " ")
			num, err := strconv.Atoi(seqStrByParts[len(seqStrByParts)-1])
			if err != nil {
				log.Fatal("In str to int cast: ", err)
			}
			seqNum = num
		} else if strings.Contains(requestByParts[i], "Authorization: ") {
			authData = requestByParts[i]
		} else if strings.Contains(requestByParts[i], "Transport: ") {
			transportData = requestByParts[i]
		} else {
			additionalData += requestByParts[i] + "\r\n"
		}
	}

	req := rtspRequest{
		reqType:         strings.ToLower(mainRequestData[0]),
		originalAddress: mainRequestData[1],
		endPoint:        strings.ToLower(addressData[len(addressData)-1]),
		receiverAddress: string(urlDecoded),
		protocolType:    mainRequestData[len(mainRequestData)-1],
		seqNum:          seqNum,
		seqStr:          seqStr,
		authHeaderData:  authData,
		additionalData:  additionalData,
		transport:       transportData,
	}

	return req
}

func createResponse(method string, req rtspRequest) string {
	result := SuccessResponse + "\r\n" + req.seqStr + "\r\n"
	switch method {
	case OPTIONS:
		result += "Public: OPTIONS, DESCRIBE, SETUP, PLAY, TEARDOWN\r\n\r\n"
	case DESCRIBE:
		result += DescribeMethodSDPHeaders + "\r\n"
	case SETUP:
		result += fmt.Sprintf("Session = %d\r\n", sessionNumber) + "\r\n" + req.transport + fmt.Sprintf(";server_port:%d-%d", ServerPort0, ServerPort1)
		sessionNumber++
	case PLAY:

	}
	return result
}

func sendResponse(conn net.Conn, req rtspRequest) {
	dataToSend := []byte(NotFoundResponse + "\n" + req.seqStr)

	if req.endPoint == "mjpeg" || req.endPoint == "stream0" {
		dataToSend = []byte(createResponse(req.reqType, req))
	}

	log.Printf(fmt.Sprintf("\x1b[%dm%s\x1b[0m", 34, string(dataToSend)))

	_, err := conn.Write(dataToSend)
	if err != nil {
		log.Fatal("In sendResponse: ", err)
	}
}

func main() {
	listen, err := net.Listen(ConnType, ConnHost+":"+ConnPort)
	if err != nil {
		log.Fatal("Listen attempt: ", err)
		os.Exit(1)
	}

	defer listen.Close()

	for true {
		conn, err := listen.Accept()
		if err != nil {
			log.Fatal("Listen attempt in loop: ", err)
			os.Exit(1)
		}

		for true {
			buffer := make([]byte, BufferSize)
			_, err := conn.Read(buffer)

			if err != nil {
				if err == io.EOF {
					log.Printf("EOF. Connection is closed.")
					break
				} else {
					log.Fatal("In handleIncomingRequest: ", err)
				}
			}

			go handleIncomingRequest(conn, buffer)
		}
	}
}
