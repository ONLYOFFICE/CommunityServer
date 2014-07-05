<%@ WebHandler Language="C#" Class="HealthHandler" %>
 
<%@ Assembly Name="System.Xml, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" %>
<%@ Assembly Name="System.Xml.Linq, Version=3.5.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089"%> 
<%@ Assembly Name="System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" %>
<%@ Assembly Name="System.Configuration, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" %>
<%@ Assembly Name="ASC.Common" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Xmpp.Common" %>


using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Configuration;
using System.Xml.Linq;
using ASC.Common.Data;
using ASC.Core;
using ASC.Xmpp.Common;

public class HealthHandler : IHttpHandler
{
    public delegate void TestMethod();

    
    public bool IsReusable
    {
        get { return true; }
    }

    
    public void ProcessRequest(HttpContext context)
    {
        Exception error = null;
        var answer = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("health"));
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        
        Test(Databases, out error);
        sw.Stop();
        answer.Root.Add(CreateModuleElement("databases", error,sw.Elapsed));
        sw.Reset();

       
        sw.Start();
        Test(Jabber, out error);
        sw.Stop();
        answer.Root.Add(CreateModuleElement("jabber", error,sw.Elapsed));

        context.Response.StatusCode = 200;
        context.Response.ContentType = "text/xml";
        answer.Save(context.Response.Output);
    }


    private bool Test(TestMethod test, out Exception error)
    { 
        try
        {
            error = null;
            test();
            return true;
        }
        catch (Exception ex)
        {
            error = ex;
            return false;
        }
    }

    private void Databases()
    {
        Hashtable alreadyChacked = new Hashtable();
        foreach (ConnectionStringSettings cs in WebConfigurationManager.ConnectionStrings)
        {
            if (!DbRegistry.IsDatabaseRegistered(cs.Name) || alreadyChacked.Contains(cs.ConnectionString)) continue;

            using (var connect = DbRegistry.CreateDbConnection(cs.Name))
            {
                connect.Open();
                connect.ExecuteScalar<int>("select 42");
            }

            alreadyChacked.Add(cs.ConnectionString, 1);
        }
    }

    private void Jabber()
    {
        if (!new JabberServiceClient().IsAvailable()) throw new Exception("Jabber service not available.");
    }

    private XElement CreateModuleElement(string module, Exception error, TimeSpan time)
    {
        var element = new XElement(module, new XAttribute("work", error == null), new XAttribute("time", time.ToString()));
        if (error != null)
        {
            element.Add(new XCData(error.ToString()));
        }
        return element;
    }
}