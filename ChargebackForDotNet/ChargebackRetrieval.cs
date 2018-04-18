﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Net;
using System.Xml.Serialization;
using ChargebackForDotNet.Properties;

namespace ChargebackForDotNet
{
    public partial class chargebackApiActivity
    {
    }

    public partial class chargebackApiCase
    {
    }
    
    public class ChargebackRetrievalRequest
    {
        private Configuration configurationField;

        public Configuration config
        {
            get
            {
                if (configurationField == null)
                {
                    // Load from Settings.
                    return null;
                }
                return this.configurationField;
            }
            set { this.configurationField = value; }
        }

        private string sendRequest(string urlRoute)
        {
            // Handle exception.
            try
            {
                List<byte> bytes = new List<byte>();
                string contentType = Communication.get(config, urlRoute, bytes);
                String xmlResponse = Utils.bytesToString(bytes);
                Console.WriteLine(xmlResponse);
                return xmlResponse;
            }
            catch (WebException we)
            {
                HttpWebResponse errorResponse = (HttpWebResponse) we.Response;
                throw new ChargebackException(
                    String.Format("Retrieval Failed - HTTP {0} Error", (int)errorResponse.StatusCode), errorResponse);
            }
        }
        
        public chargebackRetrievalResponse retrieveByActivityDate(DateTime date)
        {
            string queryDate = date.ToString("yyyy-MM-dd");
            string xmlResponse = sendRequest("/chargebacks/?date=" + queryDate);
            return Utils.DeserializeResponse<chargebackRetrievalResponse>(xmlResponse);
        }
        
        public chargebackRetrievalResponse retrieveByActivityDateWithImpact(DateTime date, bool financialImpact)
        {
            string queryDate = date.ToString("yyyy-MM-dd");
            string queryFinancialImpact = financialImpact.ToString();
            string xmlResponse = sendRequest(string.Format("/chargebacks/?date={0}&financialOnly={1}",
                queryDate, queryFinancialImpact));
            return Utils.DeserializeResponse<chargebackRetrievalResponse>(xmlResponse);
        }
        
        public chargebackRetrievalResponse retrieveActionable(bool actionable)
        {
            string queryActionable = actionable.ToString().ToLower();
            string xmlResponse = sendRequest(
                string.Format("/chargebacks/?actionable={0}", queryActionable));
            return Utils.DeserializeResponse<chargebackRetrievalResponse>(xmlResponse);
        }
        
        public chargebackRetrievalResponse retrieveByCaseId(long caseId)
        {
            string xmlResponse = sendRequest(
                string.Format("/chargebacks/{0}", caseId));
            return Utils.DeserializeResponse<chargebackRetrievalResponse>(xmlResponse);
        }
        
        public chargebackRetrievalResponse retrieveByToken(string token)
        {
            string xmlResponse = sendRequest(
                string.Format("/chargebacks/?token={0}", token));
            return Utils.DeserializeResponse<chargebackRetrievalResponse>(xmlResponse);
        }
        
        public chargebackRetrievalResponse retrieveByCardNumber(string cardNumber, DateTime expirationDate)
        {
            string queryExpirationDate = expirationDate.ToString("MMyy");
            string xmlResponse = sendRequest(
                string.Format("/chargebacks/?cardNumber={0}&expirationDate={1}",
                cardNumber, queryExpirationDate));
            return Utils.DeserializeResponse<chargebackRetrievalResponse>(xmlResponse);
        }
        
        public chargebackRetrievalResponse retrieveByArn(string arn)
        {
            string xmlResponse = sendRequest(string.Format("/chargebacks/?arn={0}",
                arn));
            return Utils.DeserializeResponse<chargebackRetrievalResponse>(xmlResponse);
        }
    }
    
    public partial class chargebackRetrievalResponse
    {
        // Additional implementation for chargebackRetrievalResponse
        // should be in here.
    }
}