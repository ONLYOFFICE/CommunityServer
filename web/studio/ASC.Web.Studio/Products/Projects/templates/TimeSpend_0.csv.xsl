<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="text" encoding="utf-8" />

  <xsl:param name="p0"/>
  <xsl:param name="p1"/>
  <xsl:param name="p2"/>
  <xsl:param name="p3"/>
  <xsl:param name="p4"/>
  <xsl:param name="p5"/>  
  <xsl:param name="p6"/>
  <xsl:param name="p7"/>
  <xsl:param name="p8"/>
  
  <xsl:decimal-format NaN = "0.00" />

  <xsl:key name="users" match="r" use="@c0" />

  <xsl:template match="*">

    <xsl:value-of select="$p0"/>
    <xsl:value-of select="$p8"/>
    
    <xsl:value-of select="$p3"/>
    <xsl:if test='($p7=-1)'>
      <xsl:value-of select="$p8"/>

      <xsl:value-of select="$p4"/>
    </xsl:if>
    <xsl:text>&#10;</xsl:text>

    <xsl:apply-templates select="r[generate-id(.)=generate-id(key('users', @c0))]"/>
    <xsl:variable name="hours" select="floor(sum(//@c5))"/>
    <xsl:variable name="minutes" select="round((sum(//@c5)-($hours))*60)"/>
    <xsl:variable name="hoursBilled" select="floor(sum(//@c6))"/>
    <xsl:variable name="minutesBilled" select="round((sum(//@c6)-($hoursBilled))*60)"/>

    <xsl:value-of select="@c8" />
    <xsl:value-of select="$p8"/>

    <xsl:value-of select="concat($hours,$p5,format-number($minutes,'00'),$p6)"/>
    <xsl:if test='($p7=-1)'>
      <xsl:value-of select="$p8"/>

      <xsl:if test='($hoursBilled=0.00)'>-</xsl:if>
      <xsl:if test='($hoursBilled!=0.00)'><xsl:value-of select="concat($hoursBilled,$p5,format-number($minutesBilled,'00'),$p6)"/></xsl:if>
    </xsl:if>
    <xsl:text>&#10;</xsl:text>
    
  </xsl:template>

  <xsl:template match="r">
    <xsl:variable name="hours" select="floor(@c5)"/>
    <xsl:variable name="minutes" select="round(((@c5)-($hours))*60)"/>
    <xsl:variable name="hoursBilled" select="floor(@c6)"/>
    <xsl:variable name="minutesBilled" select="round(((@c6)-($hoursBilled))*60)"/>
    
    <xsl:value-of select="@c8" />
    <xsl:value-of select="$p8"/>

    <xsl:value-of select="concat($hours,$p5,format-number($minutes,'00'),$p6)"/>
    <xsl:if test='($p7=-1)'>
      <xsl:value-of select="$p8"/>

      <xsl:if test='($hoursBilled=0.00)'>-</xsl:if>
      <xsl:if test='($hoursBilled!=0.00)'><xsl:value-of select="concat($hoursBilled,$p5,format-number($minutesBilled,'00'),$p6)"/></xsl:if>
    </xsl:if>
    <xsl:text>&#10;</xsl:text>
    
  </xsl:template>
</xsl:stylesheet>