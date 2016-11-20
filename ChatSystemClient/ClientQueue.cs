/* Filename     : ClientQueue.cs
 * Project      : ChatSystem/WinProgA0(4,5)
 * Author(s)    : Nathan Bray, Gabe Paquette
 * Date Created : 2016-11-17
 * Description  : This class handles methods pertaining to the message queue the client requires for the chat program
 *  it contains connecting, disconnecting, reading and location information of the queue.
 */
using BWCS;
using System;
using System.Messaging;

namespace ChatSystemClient
{
    public class ClientQueue
    {
        private const string mQueueName = @".\private$\SETQueue";
        private MessageQueue mq;

        /// <summary>
        /// This constructore starts the connection to the message queue
        /// </summary>
        public ClientQueue()
        {
            // Create and connect to the message queue
            if (!MessageQueue.Exists(mQueueName))
            {
                mq = MessageQueue.Create(mQueueName);
            }
            else
            {
                mq = new MessageQueue(mQueueName);
                // We will want to purge any messages that may have been left over from a previous MQ
                mq.Purge();
            }
        }


        /// <summary>
        /// This method will read the message body in the queue message, and return it to the main window
        /// </summary>
        public string GetMessages()
        {
            mq.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });

            // recieve the message. If there isn't one return an empty string
            string message = (string)mq.Receive().Body;
            if (message == null)
            {
                message = "";
            }
            return message;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string getPath()
        {
            return mq.Path;
        }


        /// <summary>
        /// 
        /// </summary>
        public void close()
        {
            if (MessageQueue.Exists(mQueueName))
            {
                mq.Close();
                mq.Dispose();
                MessageQueue.Delete(mq.Path);
            }
            if (ClientPipe.connected)
            {
                string message = SETMessengerUtilities.makeMessage(true, StatusCode.ClientDisconnected, MainWindow.Alias);
                ClientPipe.sendMessage(message);
                ClientPipe.disconnect();
            }
        }
    }
}

