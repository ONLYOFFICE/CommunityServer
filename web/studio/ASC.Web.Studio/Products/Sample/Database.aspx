<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Database.aspx.cs" MasterPageFile="Masters/BasicTemplate.Master" Inherits="ASC.Web.Sample.Database" %>

<%@ MasterType TypeName="ASC.Web.Sample.Masters.BasicTemplate" %>

<asp:Content ID="CommonContainer" ContentPlaceHolderID="BTPageContent" runat="server">
    <div>
        <h1>How to use database</h1>
        
        <ol>
            <li>
                <p>Create a sample table in the database</p>
<pre><code>CREATE TABLE IF NOT EXISTS `sample_table` (
    `id` INT(11) NOT NULL AUTO_INCREMENT,
    `value` VARCHAR(255) NOT NULL,
    PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;</code></pre>
                <p class="top">or add this code to the <span class="bg">..build\sql\onlyoffice.sql</span> script and run it.</p>
            </li>
            <li>
                <p>Create a data class</p>
<pre><code>public class SampleClass
{
    public int Id { get; set; }
    public string Value { get; set; }
}</code></pre>
            </li>
            <li>
                <p>Create a DAO class with the CRUD methods</p>
<pre><code>public static class SampleDao
{
    private const string DbId = "core";
    private const string Table = "sample_table";

    private static DbManager GetDb()
    {
        return new DbManager(DbId);
    }

    public static SampleClass Create(string value)
    {
        var result = new SampleClass
            {
                Value = value
            };

        using (var db = GetDb())
        {
            var query = new SqlInsert(Table, true)
                .InColumnValue("id", 0)
                .InColumnValue("value", value)
                .Identity(0, 0, true);
                    
            result.Id = db.ExecuteScalar&lt;int&gt;(query);
        }

        return result;
    }

    public static SampleClass Read(int id)
    {
        using (var db = GetDb())
        {
            var query = new SqlQuery(Table)
                .Select("id", "value")
                .Where(Exp.Eq("id", id));

            var result = db.ExecuteList(query).ConvertAll(x => new SampleClass
                {
                    Id = Convert.ToInt32(x[0]),
                    Value = Convert.ToString(x[1])
                });

            return result.Count > 0 ? result[0] : null;
        }
    }

    public static List&lt;SampleClass&gt; Read()
    {
        using (var db = GetDb())
        {
            var query = new SqlQuery(Table)
                .Select("id", "value");

            return db.ExecuteList(query).ConvertAll(x => new SampleClass
            {
                Id = Convert.ToInt32(x[0]),
                Value = Convert.ToString(x[1])
            });
        }
    }

    public static void Update(int id, string value)
    {
        using (var db = GetDb())
        {
            var existQuery = new SqlQuery(Table).SelectCount().Where(Exp.Eq("id", id));

            if (db.ExecuteScalar&lt;int&gt;(existQuery) == 0)
                throw new Exception("item not found");

            var updateQuery = new SqlUpdate(Table)
                .Set("value", value)
                .Where(Exp.Eq("id", id));

            db.ExecuteNonQuery(updateQuery);
        }
    }

    public static void Delete(int id)
    {
        using (var db = GetDb())
        {
            var query = new SqlDelete(Table).Where("id", id);

            db.ExecuteNonQuery(query);
        }
    }
}</code></pre>
            </li>
            <li>
                <p class="none">Build the project</p>
            </li>
        </ol>
    </div>
</asp:Content>
