<?xml version="1.0"?>
<configuration>
  <!-- LookAheadTarget and LookAheadInserted should line up with LookAheadIndent -->
  <appSettings>
    <add key="LookAheadInserted" value="This tag was added"/>
    <add key="LookAheadTarget" value="No indent established before this"/>   <!-- This should be on the same line as LookAheadTarget -->
    <add key="LookAheadIndent" value="We should use this tag's indent" />
    <!-- There should be no blank line after this comment -->
      <!-- This comment should be indented 2 extra spaces -->
    <!-- RemoveExtraSpace2 should line up under this comment -->
    <add key="RemoveExtraSpace2" value="This tag will remain"/>
    <!-- There should be two tags on the next line -->
    <add key="NotChanged1" value="One"/><add key="NotChanged2" value="Two"/>
    <!-- The following 3 tags should be on their own lines -->
    <add key="InsertNewLine1" value="This tag was added"/>
    <add key="InsertTarget1" value="Tags will be added before and after this"/>
    <add key="InsertNewLine2" value="This tag was added"/>
    <!-- This comment should be on the same line as /appsettings --></appSettings>
  
  
  <!-- Tags inserted inside "structure" should be indented by 5 spaces -->
  <!-- Any child tags should be indented 3 more spaces -->
 <backward>
    <tag>
   <indenting>
  <structure>
     <location path="somepath">
        <appSettings>
           <add key="SomePathSetting" value="Just a test of deep nesting"/>
        </appSettings>
     </location>
  </structure>
   </indenting>
    </tag>
 </backward>


    <location>
       <!-- Children of this appSettings element should be indented by 3 spaces -->
       <appSettings>
          <add key="NewChild3" value="New node in empty tag"/>
       </appSettings>

      <!-- Children of this appSettings element should be indented by 2 spaces -->
      <appSettings>
        <add key="NewChild4" value="New node between start and end tags"/>
      </appSettings>

	<!-- Children of this appSettings element should be indented by 4 spaces -->
	<appSettings>
	    <add key="NewChild5" value="New node between start and end tags"/>
	</appSettings>
    </location>

  <!-- A new appSettings section will be added below here -->
  <appSettings>
    <!-- This appSettings section should be added to the end 
         of the document, and all child elements including the
         comment should be indented according to local formatting
         in the source document -->
    <add key="NewChild1" value="This is the first new child element"/>
    <add key="NewChild2" value="This is the second new child element"/>
  </appSettings>
</configuration>
