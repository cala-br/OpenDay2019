__author__ = 'paolo'
import time
import http.server


HOST_NAME = 'localhost'
PORT_NUMBER = 40000


class MyHandler(http.server.BaseHTTPRequestHandler):
    def do_POST(self):
        """Respond to a GET request."""
        self.send_response(400)
        self.send_header("Content-type", "text/html")
        self.send_header("Access-Control-Allow-Origin", "*")
        self.end_headers()

if __name__ == '__main__':
    server_class = http.server.HTTPServer
    httpd = server_class((HOST_NAME, PORT_NUMBER), MyHandler)
    print (time.asctime(), "Server Starts - %s:%s" % (HOST_NAME, PORT_NUMBER))
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        pass
    httpd.server_close()
    print (time.asctime(), "Server Stops - %s:%s" % (HOST_NAME, PORT_NUMBER))