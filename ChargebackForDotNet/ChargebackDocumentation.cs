﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Xml.Serialization;
using ChargebackForDotNet.Properties;

namespace ChargebackForDotNet
{
    public class ChargebackDocumentationRequest
    {
        private Configuration configurationField;
        private Communication communication;

        public Configuration config
        {
            get { return configurationField ?? (configurationField = new Configuration()); }
            set { configurationField = value; }
        }

        public ChargebackDocumentationRequest()
        {
            communication = new Communication();
        }

        public ChargebackDocumentationRequest(Configuration config)
        {
            this.configurationField = config;
            communication = new Communication();
        }
        
        public void setCommunication(Communication comm)
        {
            communication = comm;
        }

        public chargebackDocumentUploadResponse uploadDocument(long caseId, string filePath)
        {
            List<byte> fileBytes = File.ReadAllBytes(filePath).ToList();
            string documentId = Path.GetFileName(filePath);
            try
            {
                SetUpCommunicationForUpload();
                var responseTuple = communication.post(
                    "/services/chargebacks/upload/" + caseId + "/" + documentId, fileBytes);
                var contentType = (string) responseTuple[0];
                var responseBytes = (List<byte>) responseTuple[1];
                if (contentType.Contains("application/com.vantivcnp.services-v2+xml"))
                {
                    string xmlResponse = Utils.bytesToString(responseBytes);
                    Console.WriteLine(xmlResponse);
                    chargebackDocumentUploadResponse docResponse 
                        = Utils.DeserializeResponse<chargebackDocumentUploadResponse>(xmlResponse);
                    return docResponse;
                }
                string stringResponse = Utils.bytesToString(responseBytes);
                throw new ChargebackException(
                    string.Format("Unexpected returned Content-Type: {0}. Call Vantiv immediately!" +
                                  "\nTrying to read the response as raw text:" +
                                  "\n{1}", contentType, stringResponse));
            }
            catch (WebException we)
            {
                HttpWebResponse httpResponse = (HttpWebResponse) we.Response;
                
                throw new ChargebackException("Call Vantiv. HTTP Status Code:" 
                                              + httpResponse.StatusCode
                                              + "\n" + we.Message + "\n" + we);
            }
        }

        public chargebackDocumentUploadResponse replaceDocument(long caseId, string documentId, string filePath)
        {
            List<byte> fileBytes = File.ReadAllBytes(filePath).ToList();
            try
            {
                SetUpCommunicationForUpload();
                
                var responseTuple = communication.put(
                    "/services/chargebacks/replace/" + caseId + "/" + documentId, fileBytes);
                var contentType = (string) responseTuple[0];
                var responseBytes = (List<byte>) responseTuple[1];
                if (contentType.Contains("application/com.vantivcnp.services-v2+xml"))
                {
                    string xmlResponse = Utils.bytesToString(responseBytes);
                    Console.WriteLine(xmlResponse);
                    chargebackDocumentUploadResponse docResponse
                        = Utils.DeserializeResponse<chargebackDocumentUploadResponse>(xmlResponse);
                    return docResponse;
                }
                string stringResponse = Utils.bytesToString(responseBytes);
                throw new ChargebackException(
                    string.Format("Unexpected returned Content-Type: {0}. Call Vantiv immediately!" +
                                  "\nTrying to read the response as raw text:" +
                                  "\n{1}", contentType, stringResponse));
            }
            catch (WebException we)
            {
                throw new ChargebackException("Call Vantiv. \n" + we);
            }
        }

        public IDocumentResponse retrieveDocument(long caseId, string documentId)
        {
            IDocumentResponse docResponse = null;
            try
            {
                SetUpCommunication();
                
                var responseTuple = communication.get(
                    string.Format("/services/chargebacks/retrieve/{0}/{1}", caseId, documentId));
                var contentType = (string) responseTuple[0];
                var responseBytes = (List<byte>) responseTuple[1];
                if ("image/tiff".Equals(contentType))
                {
                    var downloadDiectory = config.getConfig("downloadDirectory");
                    string filePath = Path.Combine(downloadDiectory, documentId);
                    if (!Directory.Exists(downloadDiectory))
                    {
                        Directory.CreateDirectory(downloadDiectory);
                    }
                    string retrievedFilePath = Utils.bytesToFile(responseBytes, filePath);
                    chargebackDocumentReceivedResponse fileReceivedResponse = new chargebackDocumentReceivedResponse();
                    fileReceivedResponse.retrievedFilePath = retrievedFilePath;
                    docResponse = fileReceivedResponse;
                }
                else if (contentType.Contains("application/com.vantivcnp.services-v2+xml"))
                {
                    string xmlResponse = Utils.bytesToString(responseBytes);
                    Console.WriteLine(xmlResponse);
                    docResponse 
                        = Utils.DeserializeResponse<chargebackDocumentUploadResponse>(xmlResponse);
                }
                else
                {
                    string stringResponse = Utils.bytesToString(responseBytes);
                    throw new ChargebackException(
                        string.Format("Unexpected returned Content-Type: {0}. Call Vantiv immediately!" +
                                      "\nTrying to read the response as raw text:" +
                                      "\n{1}", contentType, stringResponse));
                }
            }
            catch (WebException we)
            {
                throw new ChargebackException("Call Vantiv. \n" + we);
            }
            return docResponse;
        }

        public chargebackDocumentUploadResponse deleteDocument(long caseId, string documentId)
        {
            try
            {
                SetUpCommunication();
                
                var responseTuple = communication.delete(string.Format("/services/chargebacks/remove/{0}/{1}", caseId, documentId));
                var contentType = (string) responseTuple[0];
                var responseBytes = (List<byte>) responseTuple[1];
                if (contentType.Contains("application/com.vantivcnp.services-v2+xml"))
                {
                    string xmlResponse = Utils.bytesToString(responseBytes);
                    Console.WriteLine(xmlResponse);
                    chargebackDocumentUploadResponse docResponse
                        = Utils.DeserializeResponse<chargebackDocumentUploadResponse>(xmlResponse);
                    return docResponse;
                }
                string stringResponse = Utils.bytesToString(responseBytes);
                throw new ChargebackException(
                    string.Format("Unexpected returned Content-Type: {0}. Call Vantiv immediately!" +
                                  "\nTrying to read the response as raw text:" +
                                  "\n{1}", contentType, stringResponse));
                              
            }
            catch (WebException we)
            {
                throw new ChargebackException("Call Vantiv. \n" + we);
            }
        }

        public chargebackDocumentUploadResponse listDocuments(long caseId)
        {
            try
            {
                SetUpCommunication();
                
                var responseTuple = communication.get("/services/chargebacks/list/" + caseId);
                var contentType = (string) responseTuple[0];
                var responseBytes = (List<byte>) responseTuple[1];
                if (contentType.Contains("application/com.vantivcnp.services-v2+xml"))
                {
                    string xmlResponse = Utils.bytesToString(responseBytes);
                    Console.WriteLine(xmlResponse);
                    chargebackDocumentUploadResponse docResponse
                        = Utils.DeserializeResponse<chargebackDocumentUploadResponse>(xmlResponse);
                    return docResponse;
                }
                string stringResponse = Utils.bytesToString(responseBytes);
                throw new ChargebackException(
                    string.Format("Unexpected returned Content-Type: {0}. Call Vantiv immediately!" +
                                  "\nTrying to read the response as raw text:" +
                                  "\n{1}", contentType, stringResponse));
                              
            }
            catch (WebException we)
            {
                throw new ChargebackException("Call Vantiv. \n" + we);
            }
        }

        
        private void SetUpCommunication()
        {
            communication.setHost(config.getConfig("host"));
            string encoded = Utils.encode64(config.getConfig("username") + ":" + config.getConfig("password"), "utf-8");
            communication.addToHeader("Authorization", "Basic " + encoded);
            communication.setProxy(config.getConfig("proxyHost"), int.Parse(config.getConfig("proxyPort")));
            communication.setContentType(null);
        }
        
        private void SetUpCommunicationForUpload()
        {           
            SetUpCommunication();
            communication.setContentType("image/tiff");
        }
    }
}