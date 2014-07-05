<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="http://www.w3.org/1999/xhtml">
  <xsl:output method="html" />

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
    <xsl:variable name="hours" select="floor(sum(//@c5))"/>
    <xsl:variable name="minutes" select="round((sum(//@c5)-($hours))*60)"/>
    <xsl:variable name="hoursBilled" select="floor(sum(//@c6))"/>
    <xsl:variable name="minutesBilled" select="round((sum(//@c6)-($hoursBilled))*60)"/>
    
    <div id="reportBody">
      <table class="sortable pm-tablebase" cellspacing="0" cellpadding="10" id="result">
        <thead>
          <tr>
            <td style="width:69%;text-align:left;">
              <span class="report-tableColumnHeader" unselectable="on">
                <xsl:attribute name="title">
                  <xsl:value-of select="$p3"/>
                </xsl:attribute>
                <xsl:value-of select="$p0"/>
              </span>
              <span id="sorttable_sortfwdind"></span>
            </td>
            <td style="width:15%;text-align: center;white-space: nowrap;">
              <span class="report-tableColumnHeader" unselectable="on">
                <xsl:attribute name="title">
                  <xsl:value-of select="$p3"/>
                </xsl:attribute>
                <xsl:value-of select="$p1"/>
              </span>
            </td>
            <xsl:if test='($p8=-1)'>
              <td style="width:15%;text-align: center;white-space: nowrap;">
                <span class="report-tableColumnHeader" unselectable="on">
                  <xsl:attribute name="title">
                    <xsl:value-of select="$p3"/>
                  </xsl:attribute>
                  <xsl:value-of select="$p2"/>
                </span>
              </td>
            </xsl:if>
          </tr>
        </thead>

        <tbody>
          <xsl:apply-templates select="r[generate-id(.)=generate-id(key('users', @c0))]"/>
          <tr class="sortbottom">
            <td class='borderBase' style="width:69%">
              <div style='width:99%;overflow:hidden;'>
              </div>
            </td>
            <td class="borderBase" style="width:15%; text-align: center;">
              <div style='width:99%;overflow:hidden;font-size:16px;font-weight:bold;'>
                <xsl:value-of select="concat($hours,$p6,' ',format-number($minutes,'00'),$p7)"/>
              </div>
            </td>
            <xsl:if test='($p8=-1)'>
            <td class="borderBase" style="width:15%; text-align: center;">
              <div style='width:99%;overflow:hidden;font-size:16px;font-weight:bold;'>
                <xsl:if test='($hoursBilled=0.00)'>
                  -
                </xsl:if>
                <xsl:if test='($hoursBilled!=0.00)'>
                  <xsl:value-of select="concat($hoursBilled,$p6,' ',format-number($minutesBilled,'00'),$p7)"/>
                </xsl:if>
              </div>
            </td>
            </xsl:if>
          </tr>
        </tbody>
      </table>
    </div>
  </xsl:template>

  <xsl:template match="r">
    <xsl:variable name="hours" select="floor(@c5)"/>
    <xsl:variable name="minutes" select="round(((@c5)-($hours))*60)"/>
    <xsl:variable name="hoursBilled" select="floor(@c6)"/>
    <xsl:variable name="minutesBilled" select="round(((@c6)-($hoursBilled))*60)"/>
    <tr>
      <td class='borderBase'>
        <div style='width:99%;overflow:hidden;'>
          <a href="{$p4}{@c9}">
            <xsl:value-of select="@c8" />
          </a>
        </div>
      </td>
      <td class="borderBase time" style="text-align: center;">
        <div style='width:99%;overflow:hidden;'>
          <xsl:value-of select="concat($hours,$p6,format-number($minutes,'00'),$p7)"/>
        </div>
          <span style="display:none;">
              <xsl:value-of select="$hours*60 + $minutes"/>
          </span>
      </td>
      <xsl:if test='($p8=-1)'>
      <td class="borderBase time" style="text-align: center;">
        <div style='width:99%;overflow:hidden;'>
          <xsl:if test='($hoursBilled=0.00)'>
            -
          </xsl:if>
          <xsl:if test='($hoursBilled!=0.00)'>
            <xsl:value-of select="concat($hoursBilled,$p6,' ',format-number($minutesBilled,'00'),$p7)"/>
          </xsl:if>
        </div>
          <span style="display:none;">
              <xsl:value-of select="$hoursBilled*60 + $minutesBilled"/>
          </span>
      </td>
      </xsl:if>
    </tr>
  </xsl:template>
  
</xsl:stylesheet>