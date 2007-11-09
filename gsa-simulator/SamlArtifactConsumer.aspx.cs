/*
 * Copyright (C) 2006 Google Inc.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy of
 * the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Net;
using System.IO;
using System.Xml;

namespace gsa
{
	/// <summary>
	/// Summary description for SamlArtifactConsumer.
	/// </summary>
	public class SamlArtifactConsumer : System.Web.UI.Page
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			Common.log("gsa::SamArtifactConsumer_Page_Load");
			String subject = GetSubject();
			String relay = Request.Params["RelayState"];
			//we now extract the url from the relayState, 
			//this is not GSA behavior, it's for simulation
			relay = HttpUtility.UrlDecode(relay);
			int idx = relay.IndexOf("&Resource=");
			Common.log("&resource=" + relay);
			String res = relay.Substring(idx + 10);
			idx = res.IndexOf("&");
			res = res.Substring(0, idx);
			if (subject == null)
			{
				Response.Write("failed");
				return;
			}
			Authz(subject, res);
		}

		String GetSubject()
		{
			Common.log("gsa::SamArtifactConsumer:getSubject");
			String art = Request.Params["SAMLart"];
			String req = Common.SamlResolveTemplate;
			req = req.Replace("%ARTIFACT", art);
			req = req.Replace("%INSTANT", Common.FormatNow());
			req = req.Replace("%ISSUER", Server.MachineName);
			req = req.Replace("%ID", Common.GenerateRandomString());
			String res = Common.PostToURL(Common.GetSamlResolver(Request), req);
			XmlDocument doc = new XmlDocument();
			doc.InnerXml = res;
			XmlNodeList list = doc.GetElementsByTagName("Subject");
			XmlNode subject = list.Item(0);
			String sub = subject.InnerXml;
			int idx = sub.IndexOf("CN=");
			sub = sub.Substring(idx + 3);
			idx = sub.IndexOf("<");
			sub = sub.Substring(0, idx);
			return sub;
		}

		void Authz(String subject, String resource)
		{
			Common.log("gsa::SamArtifactConsumer:Authz");
			//now authorize the url
			String relay =	Request.Params[Common.RelayState];
			String req = Common.AuthzTemplate;
			req = req.Replace("%INSTANT", Common.FormatNow());
			req = req.Replace("%ID", Common.GenerateRandomString());
			req = req.Replace("%RESOURCE", resource);
			req = req.Replace("%SUBJECT", subject);
			try
			{
				String res = Common.PostToURL(Common.GetAuthorizer(Request), req);
				Response.ContentType = "text/xml";
				Response.Write(res);			
			}
			catch(Exception e)
			{
				Common.log(e.Message);
			}
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
			{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}