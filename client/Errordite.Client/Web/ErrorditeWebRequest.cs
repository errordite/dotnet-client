using System;
using System.Net;

namespace Errordite.Client.Web
{
    internal class ErrorditeWebRequest
    {
        private int _timeout = 30000;
        private ClientError _error;
        private readonly string _requestUri;

        private ErrorditeWebRequest(string requestUri)
        {
            _requestUri = requestUri;
        }

        /// <summary>
        /// Initiate the fluent interface of the ErrorditeWebRequest object by specifying the URI you want to invoke
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static ErrorditeWebRequest To(string uri)
        {
            return new ErrorditeWebRequest(uri);
        }

        public ErrorditeWebRequest WithError(ClientError error)
        {
            _error = error;
            return this;
        }

        public ErrorditeWebRequest TimeoutIn(int seconds)
        {
            _timeout = seconds;
            return this;
        }

        public void Send(bool gzip) //todo: gzip request
        {
            byte[] bytes = ErrorditeSerializer.Serialize(_error);

            Uri uri = new Uri(_requestUri);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = WebConstants.HttpMethods.Post;

#if NET2
            request.ContentType = "application/xml; charset=utf-8";
#else
            request.ContentType = "application/json; charset=utf-8";
#endif
            request.Timeout = _timeout * 1000;
            request.UserAgent = "Errordite";

            //set the content length
            request.ContentLength = bytes.Length;

            //write to the request stream
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            using(request.GetResponse())
            {}
        }
    }
}
