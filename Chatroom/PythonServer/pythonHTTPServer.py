import time
import http.server
import cgi
import subscriptionManager


HOST_NAME = 'localhost'
PORT_NUMBER = 40000
sub = subscriptionManager.SubscriptionManager()


class MyHandler(http.server.BaseHTTPRequestHandler):
    def do_POST(self):
        """Respond to a GET request."""
        user = self.rfile.read(int(self.headers['Content-Length'])).decode()

        if self.path == '/deregister':
            sub.deregisterClient(user)
            self.send_response(200)
        
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