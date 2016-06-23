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
  <xsl:param name="p9"/>
  
  <xsl:key name="g1" match="r" use="@c8"/>
  <xsl:key name="g2" match="r" use="concat(@c8, '|', @c0)"/>
  <xsl:key name="g3" match="r" use="concat(@c8, '|', @c0, '|', @c2)"/>

  <xsl:template match="*">

    <xsl:value-of select="$p7"/>
    <xsl:value-of select="$p9"/>
    
    <xsl:value-of select="$p0"/>
    <xsl:value-of select="$p9"/>

    <xsl:value-of select="$p1"/>
    <xsl:value-of select="$p9"/>

    <xsl:value-of select="$p2"/>
    <xsl:value-of select="$p9"/>

    <xsl:value-of select="$p3"/>
    <xsl:value-of select="$p9"/>

    <xsl:value-of select="$p4"/>
    <xsl:value-of select="$p9"/>

    <xsl:value-of select="$p5"/>
    <xsl:value-of select="$p9"/>

    <xsl:value-of select="$p6"/>
    <xsl:text>&#10;</xsl:text>

    <xsl:for-each select="//r[generate-id() = generate-id(key('g1', @c8))]">
      
      <xsl:sort select="@c11" />
      
      <xsl:variable name="user" select="@c8"/>

      <xsl:for-each select="key('g1', $user) [generate-id() = generate-id(key('g2', concat($user, '|', @c0)))]">
        
        <xsl:sort select="@c1"/>
        
        <xsl:variable name="proj" select="@c0"/>

        <xsl:for-each  select="key('g2', concat($user, '|', $proj)) [generate-id() = generate-id(key('g3', concat($user, '|', @c0, '|', @c2)))]">
          
          <xsl:variable name="milestone" select="@c2"/>

          <xsl:for-each select="../r[@c8 = $user and @c0 = $proj and @c2 = $milestone]">

            <xsl:value-of select="@c12" />
            <xsl:value-of select="$p9"/>

            <xsl:value-of select="@c1" />
            <xsl:value-of select="$p9"/>

            <xsl:if test="@c2 = '0'">
              <xsl:value-of select="$p8"/>
            </xsl:if>
            <xsl:if test="@c2 != '0'">
              <xsl:value-of select="@c3"/>
            </xsl:if>
            <xsl:value-of select="$p9"/>

            <xsl:value-of select="@c4" />
            <xsl:value-of select="$p9"/>

            <xsl:if test="@c5 != '-1'">
              <xsl:value-of select="@c5"/>
            </xsl:if>
            <xsl:value-of select="$p9"/>

            <xsl:value-of select="@c7" />
            <xsl:value-of select="$p9"/>

            <xsl:value-of select="@c10" />
            <xsl:value-of select="$p9"/>

            <xsl:value-of select="@c9" />
            <xsl:text>&#10;</xsl:text>

          </xsl:for-each>

        </xsl:for-each>

      </xsl:for-each>

    </xsl:for-each>

  </xsl:template>

</xsl:stylesheet>