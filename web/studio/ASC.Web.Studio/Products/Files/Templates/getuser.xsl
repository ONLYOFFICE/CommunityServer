<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" encoding="utf-8" standalone="yes" indent="yes" omit-xml-declaration="yes" media-type="text/xhtml" />

  <register type="ASC.Web.Files.Resources.FilesCommonResource,ASC.Web.Files" alias="fres" />

  <xsl:template match="userItem">
    <div class="ds-user-link">
      <span class="ds-user-link-delete">
        <xsl:attribute name="title">
          <resource name="fres.ButtonDelete" />
        </xsl:attribute>
      </span>
      <span class="userLink">
        <xsl:attribute name="id">userItemLink_<xsl:value-of select="id" /></xsl:attribute>
        <xsl:attribute name="data-uid">
          <xsl:value-of select="id" />
        </xsl:attribute>
        <xsl:value-of select="decodedTitle" />
      </span>
    </div>
  </xsl:template>

</xsl:stylesheet>