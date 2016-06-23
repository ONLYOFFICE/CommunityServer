<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="http://www.w3.org/1999/xhtml">
  <xsl:output method="html" />

  <xsl:param name="p0"/>
  
  <xsl:template match="*">

    <div class="noContentBlock" id="reportBody">
      <div>
      </div>
      <div style='font-size: 14px; margin-top: 5px;'>
        <xsl:value-of select="$p0"/>
      </div>
    </div>
  </xsl:template>

</xsl:stylesheet>