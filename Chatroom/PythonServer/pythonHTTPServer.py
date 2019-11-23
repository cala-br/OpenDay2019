import time
import json
import html
from typing import List
from http.server import \
    BaseHTTPRequestHandler, HTTPServer
    
from subscriptionManager import \
    SubscriptionManager


#region Subscription Handler
class SubscriptionHandler(BaseHTTPRequestHandler):

    #region Static fields

    subscriptionManager = SubscriptionManager()

    #endregion


    #region Get 
    def do_GET(self):
        """
        Responds to
        -----------
            '/getNames'
                200 - List[str]
        """
        sm = \
            SubscriptionHandler.subscriptionManager

        print(self.path)
        data = b'["error"]'
        if self.path == '/getNames':
            data = json\
                .dumps(sm.usernames)\
                .encode()

        self.send_response(200)
        self.send_header("Content-type", "text/html")
        self.send_header("Access-Control-Allow-Origin", "*")
        self.end_headers()
        self.wfile.write(data)
    #endregion

    #region Post
    def do_POST(self):
        """
        Responds to
        -----------
            '/deregister', with param 'user'
                200 

            '/register', with param 'user'
                200 -> Done
                400 -> Username exists
        """
        sm = \
            SubscriptionHandler.subscriptionManager

        user = self\
            .rfile\
            .read(int(self.headers['Content-Length']))\
            .decode()

        user = html.escape(user)

        if self.path == '/deregister':
            sm.deregisterClient(user)
            self.send_response(200)

        elif self.path == '/register':
            registerResponse = \
                sm.registerClient(user)

            self.send_response(
                200 if registerResponse else 400)
        
        self.send_header("Content-type", "text/html")
        self.send_header("Access-Control-Allow-Origin", "*")
        self.end_headers()
    #endregion

#endregion

#region Main
def main():
    """
    Initializes the server.
    """
    HOST_NAME   = ''
    PORT_NUMBER = 40000

    server = HTTPServer(
        (HOST_NAME, PORT_NUMBER), 
        SubscriptionHandler)

    try:
        print(
            f"{time.asctime()} -> Server Starts - '{HOST_NAME}', {PORT_NUMBER}")

        server.serve_forever()

    except:    
        server.server_close()
        print(
            f"{time.asctime()} -> Server Stops - '{HOST_NAME}', {PORT_NUMBER}")
#endregion


if __name__ == '__main__':
    try:
        main()
    except:
        pass