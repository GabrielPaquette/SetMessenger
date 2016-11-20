/*
Project: ChatSystemService - ChatServer.cs
Developer(s): Gabriel Paquette, Nathaniel Bray
Date: November 19, 2016
Description: This file contains the code that runs the server for the chat system.
             A new thread is spawned for each new client that is connected to the 
             server. Each message recieved from the client is processed to determine
             the appropriate action to take.
*/

using System;
using System.Collections.Generic;
using System.Messaging;
using System.Text;
using BWCS;
using System.Threading;
using System.IO.Pipes;
using System.IO;

namespace ChatSystemService
{
    class ChatServer
    {
        //flag to determine if the server will continue to run
        private bool closeServerFlag = false;
        //a list of users, name is key, machine number is value
        private Dictionary<string, string> userList = new Dictionary<string, string>();

        /*
        Name: startServer
        Description: this function calls the processNextClient thread, which handles connecting
                     new users to the server. It sleeps for 1 second after a client connects
                     to allow the service to complete it's oporations.
        */
        public void startServer()
        {
            do
            {
                processNextClient();
                //sleep to allow the service to do what it needs to do
                Thread.Sleep(1000);
                //while the close server flag is false, continue to accept new clients 
            } while (!closeServerFlag);
        }


        /*
        Name: ProcessClientThread
        Parameters: object pStream -> this is the server named pipe that holds the 
                                      connection between the server and the specific
                                      client.
        Description: This function reads in a message from the named pipe, and send it
                     to a function that determines what to do with the message. If the 
                     closeClientThreadFlag is false, then it will continue to read messages
                     through the pipe from the client.
        */
        private void ProcessClientThread(object pStream)
        {
            NamedPipeServerStream pipeStream = (NamedPipeServerStream)pStream;

            //flag to determine if this client thread should stay running
            bool closeClientThreadFlag = false;

            while (closeServerFlag == false && closeClientThreadFlag == false)
            {
                try
                {
                    var recievedByteMessage = new byte[1024];
                    string message = "";

                    //read the message sent through the pipe
                    pipeStream.Read(recievedByteMessage, 0, 1024);
                    //convert the message into a string and cut out the \0s at the end of the string
                    message = Encoding.ASCII.GetString(recievedByteMessage).TrimEnd('\0');

                    //message = message.Substring(0, message.IndexOf('\0'));
                    //determine what to do with the message recieved and does the action needed
                    processMessageRecieved(message, out closeClientThreadFlag);

                }
                catch (IOException e)
                {
                    Logger.Log("Server-ProcessClientThread FileError: " + e.Message);
                }
                catch (Exception e)
                {
                    closeClientThreadFlag = true;
                    Logger.Log("Server - ProcessClientThread Error" +e.Message);
                }
            }

            //close the pipe
            pipeStream.Close();
            //dispose of and release the pipe
            pipeStream.Dispose();
        }


        /*
        Name: processMessageRecieved
        Parameters: string message -> this is the message recieved from the client
                    out bool ct-> this is the close thread flag
        Description: This function is passed a message that was read in from the pipe.
                     The message is then split up, to determine what actions need to
                     be taken. The number in messageInfo[1] determines what action
                     needs to be taken.
        */
        private void processMessageRecieved(string message, out bool ct)
        {
            bool closeThread = false;
            char[] delim = { ':' };
            //break up the string
            string[] messageInfo = message.Split(delim, 5, StringSplitOptions.RemoveEmptyEntries);
            StatusCode sc = (StatusCode)int.Parse(messageInfo[1]);

            //determine what needs to be done
            switch (sc)
            {
                case StatusCode.ClientConnected:
                    //userName, machineName
                    processClientConnect(messageInfo[2], messageInfo[0]);
                    break;
                case StatusCode.Whisper:
                    //From, To, Message
                    processWhisper(messageInfo[2], messageInfo[3], messageInfo[4]);
                    break;
                case StatusCode.All:
                    //From, Message
                    processBroadcast(messageInfo[2], messageInfo[4]);
                    break;
                case StatusCode.ClientDisconnected:
                    //From/who
                    processClientDisconnect(messageInfo[2]);
                    closeThread = true;
                    break;
                case StatusCode.ServerClosing:
                    //close server
                    processServerClose();
                    break;
            }

            ct = closeThread;
        }


        /*
        Name: ProcessNextClient
        Description: This function waits for a connection with a client. Once a connection is
                     made, it passes the pipe stream to a process client thread
        */
        private void processNextClient()
        {
            try
            {
                NamedPipeServerStream pipeStream = new NamedPipeServerStream(SETMessengerUtilities.pipeName, PipeDirection.In, 254);
                pipeStream.WaitForConnection();

                //Spawn a new thread for each request and continues wait for another client to connect
                Thread t = new Thread(ProcessClientThread);
                t.Start(pipeStream);
            }
            catch (Exception e)
            {
                //If there are no more available connections (254 is in use already) then just keep looping until one is available
                Logger.Log("Server-ProccesNextClient Error: " + e.Message);
            }
        }


        /*
        Name: processWhisper
        Parameters: string from -> this is the user name of the client that sent the message
                    string to -> this is the username of the client that will recieve the sent message
                    string message -> this is the message to be sent
        Description: This function looks up the machine name of the client that is to recieve the message.
                     It then sends to message to that client, tagging the message with the name of the user
                     who sent the message.
        */
        private void processWhisper(string from, string to, string message)
        {
            string machineName = "";

            if (userList.TryGetValue(to, out machineName) == true)
            {
                //contruct the whisper to send
                string messageToSend = SETMessengerUtilities.makeMessage(false, StatusCode.Whisper, from, message);
                //send the message
                sendMsg(messageToSend, machineName);
            }
        }


        /*
        Name: processBroadcast
        Parameters: string from -> this is the user name of the client that sent the message
                    string message -> this is the message to be sent to everyone on the user list
        Description: This function sends a message to everyone on the user list, and tags the message
                     with the user name of the client who sent it.
        */
        private void processBroadcast(string from, string message)
        {
            string messageToSend = SETMessengerUtilities.makeMessage(false, StatusCode.All, from, message);
            sendBroadcastMessage(messageToSend);
        }


        /*
        Name: processClientDisconnect
        Parameters: string name -> this is the username of the client that disconnected from the server
        Description: This function creates the 
        */
        private void processClientDisconnect(string who)
        {
            if (userList.ContainsKey(who))
            {
                string disconnectMessage = SETMessengerUtilities.makeMessage(false, StatusCode.ClientDisconnected, who);
                //delete user when disconnect
                userList.Remove(who);

                sendBroadcastMessage(disconnectMessage);
            }
        }


        /*
        Name: processClientConnect
        Parameters: string name -> this is the username of the client that connected
                    string machineName -> this is the machine name of the client that connected
        Description: This function adds the client -that just connected to the server- to the user list.
                     The name is the key, and the machine name is the value. A message is then
                     sent to everyone in the user list, saying this new user connected to the server.
                     The whole user list is then sent to the new client.
        */
        private void processClientConnect(string name, string machineName)
        {
            //adds this user to the user list
            userList.Add(name, machineName);

            string connectMessage = SETMessengerUtilities.makeMessage(false, StatusCode.ClientConnected, name);
            sendBroadcastMessage(connectMessage);

            sendUserlist(machineName);
        }


        /*
         Name: processServerClose
         Description: This function sends a message to each client in the user list,
                      saying the server is shuting down. The closingserverflag is then 
                      set to true, so the server can exit cleanly.
         */
        public void processServerClose()
        {
            string serverClosingMessage = SETMessengerUtilities.makeMessage(false, StatusCode.ServerClosing, "Closing server");
            sendBroadcastMessage(serverClosingMessage);
            closeServerFlag = true;
        }


        /*
        Name: sendBroadcastMessage
        Parameters: string messageToBroadcast -> this is the message that will be sent to everyone on the user list
        Description: this message sends out a message to everyone in the user list
        */
        private void sendBroadcastMessage(string messageToBroadcast)
        {
            foreach (var item in userList)
            {
                sendMsg(messageToBroadcast, item.Value);
            }
        }


        /*
        Name: sendUserlist
        Parameters: string machineName -> this is the machine name of the user who will recieve the full userlist
        Description: The function creates a new message with every username in it, 
                     and then sends that message to the client that matches the machineName
        */
        private void sendUserlist(string machineName)
        {
            string userListMessage = ":" + (int)StatusCode.SendUserList + ":";

            foreach (string name in userList.Keys)
            {
                userListMessage += name + ":";

            }
            //send updatedUserList to the message queue using machineName
            sendMsg(userListMessage, machineName);
        }


        /*
        Name: sendMsg
        Parameters: string message -> this is the message that will be sent
                    string machineName -> this is the machineName of the client that will recieve the sent message
        Description: This function is passed a message and a machine name. The message is
                     sent to the client that has that machine name
        */
        private void sendMsg(string message, string machineName)
        {
            MessageQueue mq = new MessageQueue("FormatName:DIRECT=OS:" + machineName + "\\Private$\\SETQueue");
            mq.Send(message);            
        }
    }
    //http://stackoverflow.com/questions/4570653/multithreaded-namepipeserver-in-c-sharp
}
