<?xml version="1.0"?>
<configuration>
  <connectionStrings>
    <!-- Attributes on this element should be separated by single spaces -->
    <add name="AddingScenario1" connectionString="Just ask Joe, he'll do it" providerName="Joe"/>
    <!-- Attributes on this element should be separated by spaces, with providerName wrapped using a generated tab -->
    <add name="AddingScenario2" connectionString="Just ask Joe, he'll do it"
      providerName="Joe, the great provider of data who will provide your data for you!!!!!"/>
  </connectionStrings>

  <connectionStrings>
    <!-- Attributes on this element should be one per line ('foo' attribute is added on a new line) -->
    <add  name="AddingScenario5"
          connectionString="Data Source=JODAVIS-DEV1;Initial Catalog=Northwind;Integrated Security=True"
          providerName="System.Data.SqlClient"
          foo="foo" />
    <!-- 'foo' attribute on this element should be added on the same line -->
    <add  name="AddingScenario6"   connectionString="Data Source=JODAVIS-DEV1;Initial Catalog=Northwind;Integrated Security=True" foo="foo" />
    <!-- 'foo' attribute on this element should be added on the same line as 'providerName' -->
    <add  name="AddingScenario7"   connectionString="Data Source=JODAVIS-DEV1;Initial Catalog=Northwind;Integrated Security=True"
          providerName="System.Data.SqlClient" foo="foo" />
    <!-- 'foo' attribute on this element should be added on the same line as 'providerName' -->
    <add  name="AddingScenario8"
          connectionString="Data Source=JODAVIS-DEV1;Initial Catalog=Northwind;Integrated Security=True" sniggle="sniggle"
          providerName="System.Data.SqlClient" foo="foo" />

    <!-- There should be two spaces before 'connectionString' -->
    <add  connectionString="Data Source=JODAVIS-DEV1;Initial Catalog=Northwind;Integrated Security=True"    providerName="System.Data.SqlClient" />
    <!-- There should be two spaces and no new line before 'connectionString' -->
    <add  connectionString="Data Source=JODAVIS-DEV1;Initial Catalog=Northwind;Integrated Security=True"    providerName="System.Data.SqlClient" />
    <!-- There should be two spaces before 'foo' -->
    <add  foo="foo" />
    <!-- There should be two spaces before 'name' and four before 'providerName' -->
    <add  name="RemovingScenario4"    providerName="System.Data.SqlClient" />
    <!-- There should be two spaces before 'name' and 'providerName' should be lined up under 'name' -->
    <add  name="RemovingScenario5"   
          providerName="System.Data.SqlClient" />
    <!-- There should be two spaces before 'name' and 'providerName' should be lined up under 'add' -->
    <add  name="RemovingScenario6"    
     providerName="System.Data.SqlClient" />
    <!-- There should be two spaces before 'name' and 'foo' should be lined up under 'name' -->
    <add  name="RemovingScenario7"   
          foo="foo" />
    <!-- There should be two spaces before 'name' and three before 'connectionString' -->
    <add  name="RemovingScenario8"   connectionString="Data Source=JODAVIS-DEV1;Initial Catalog=Northwind;Integrated Security=True" />
    <!-- There should be two spaces before 'name' and three before 'connectionString' -->
    <add  name="RemovingScenario9"   connectionString="Data Source=JODAVIS-DEV1;Initial Catalog=Northwind;Integrated Security=True" />

    <!-- There should be two spaces before 'foo' -->
    <add  foo="foo" />
    <!-- There should be two spaces before 'connectionString' and 'foo' should be added on the same line -->
    <add  connectionString="Data Source=JODAVIS-DEV1;Initial Catalog=Northwind;Integrated Security=True"    providerName="System.Data.SqlClient" foo="foo" />
    <!-- There should be four spaces before 'providerName' and one space before 'foo' -->
    <add  name="AddAndRemoveScenario3"    providerName="System.Data.SqlClient" foo="foo" />
    <!-- There should be three spaces before 'connectionString' and four spaces before 'providerName' -->
    <add  name="AddAndRemoveScenario4"   connectionString="foo"    providerName="System.Data.SqlClient" />
    <!-- There should be three spaces before 'connectionString' and one space before 'foo' -->
    <add  name="AddAndRemoveScenario5"   connectionString="Data Source=JODAVIS-DEV1;Initial Catalog=Northwind;Integrated Security=True" foo="foo" />
    <!-- There should be three spaces before 'connectionString' and four spaces before 'providerName' -->
    <add  name="AddAndRemoveScenario6"   connectionString="Data Source=JODAVIS-DEV1;Initial Catalog=Northwind;Integrated Security=True"    providerName="foo" />
  </connectionStrings>

  <!-- AddingScenario3 should get newline from NewLineProvider2 -->
  <connectionStrings>
    <add name="NewLineProvider1" connectionString="Data Source=JODAVIS-DEV1;Initial Catalog=Northwind;Integrated Security=True"
      providerName="System.Data.SqlClient" />
    <add name="NewLineProvider2" connectionString="Data Source=JODAVIS-DEV1;Initial Catalog=Northwind;Integrated Security=True"
       providerName="System.Data.SqlClient" />
    <add name="AddingScenario3" connectionString="Just ask Joe, he'll do it"
       providerName="Joe, the great provider of data who will provide your data for you!!!!!"/>
    <add name="NewLineProvider3" connectionString="Data Source=JODAVIS-DEV1;Initial Catalog=Northwind;Integrated Security=True"
        providerName="System.Data.SqlClient" />
    <add name="NewLineProvider4" connectionString="Data Source=JODAVIS-DEV1;Initial Catalog=Northwind;Integrated Security=True"
         providerName="System.Data.SqlClient" />
  </connectionStrings>

  <!-- AddingScenario4 should get newline from NewLineProvider3 -->
  <connectionStrings>
    <add name="NewLineProvider1" connectionString="Data Source=JODAVIS-DEV1;Initial Catalog=Northwind;Integrated Security=True" providerName="System.Data.SqlClient" />
    <add name="NewLineProvider2" connectionString="Data Source=JODAVIS-DEV1;Initial Catalog=Northwind;Integrated Security=True" providerName="System.Data.SqlClient" />
    <add name="AddingScenario4" connectionString="Just ask Joe, he'll do it"
      providerName="Joe, the great provider of data who will provide your data for you!!!!!"/>
    <add name="NewLineProvider3" connectionString="Data Source=JODAVIS-DEV1;Initial Catalog=Northwind;Integrated Security=True"
      providerName="System.Data.SqlClient" />
    <add name="NewLineProvider4" connectionString="Data Source=JODAVIS-DEV1;Initial Catalog=Northwind;Integrated Security=True"
         providerName="System.Data.SqlClient" />
  </connectionStrings>
</configuration>
