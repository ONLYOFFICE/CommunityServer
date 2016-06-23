<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
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

  <xsl:template match="/">
    
    <div id="reportBody">
      <table class="sortable pm-tablebase" cellspacing="0" cellpadding="10" id="result">
        <thead>
          <tr>
            <td style="width:44%;text-align:left;">
              <span class="report-tableColumnHeader" unselectable="on">
                <xsl:attribute name="title">
                  <xsl:value-of select="$p6"/>
                </xsl:attribute>
                <xsl:value-of select="$p0"/>
              </span>
              <span id="sorttable_sortfwdind"></span>
            </td>
            <td style="width:15%;text-align:left;white-space:nowrap;">
              <span class="report-tableColumnHeader" unselectable="on">
                <xsl:attribute name="title">
                  <xsl:value-of select="$p6"/>
                </xsl:attribute>
                <xsl:value-of select="$p1"/>
              </span>
            </td>
            <td style="width:10%;text-align:left;">
              <span class="report-tableColumnHeader" unselectable="on">
                <xsl:attribute name="title">
                  <xsl:value-of select="$p6"/>
                </xsl:attribute>
                <xsl:value-of select="$p2"/>
              </span>
            </td>
            <td style="width:10%;text-align:left;">
              <span class="report-tableColumnHeader" unselectable="on">
                <xsl:attribute name="title">
                  <xsl:value-of select="$p6"/>
                </xsl:attribute>
                <xsl:value-of select="$p3"/>
              </span>
            </td>
            <td style="width:10%;text-align:left;">
              <span class="report-tableColumnHeader" unselectable="on">
                <xsl:attribute name="title">
                  <xsl:value-of select="$p6"/>
                </xsl:attribute>
                <xsl:value-of select="$p4"/>
              </span>
            </td>
            <td style="width:10%;text-align:left;">
              <span class="report-tableColumnHeader" unselectable="on">
                <xsl:attribute name="title">
                  <xsl:value-of select="$p6"/>
                </xsl:attribute>
                <xsl:value-of select="$p5"/>
              </span>
            </td>
          </tr>
        </thead>

        <tbody>

          <xsl:apply-templates/>

        </tbody>
      </table>
    </div>
    
  </xsl:template>

  <xsl:template match="r">

    <tr>
      <td class="borderBase" style="width:44%">
        <div style='width:99%;overflow:hidden;'>
          <a href="{$p7}{$p8}/products/projects/projects.aspx?prjID={@c0}">
            <xsl:value-of select="@c1" />
          </a>
        </div>
      </td>
      <td class="borderBase" style="width:15%">
        <div style='width:99%;overflow:hidden;white-space:nowrap;'>
          <a href="{$p7}{@c8}">
            <xsl:value-of select="@c7" />
          </a>
        </div>
      </td>
      <td class="borderBase" style="width:10%">
        <div style='width:99%;overflow:hidden;'>
          <xsl:value-of select="@c3" />
        </div>
      </td>
      <td class="borderBase" style="width:10%">
        <div style='width:99%;overflow:hidden;'>
          <a href="{$p7}{$p8}/products/projects/milestones.aspx?prjID={@c0}">
            <xsl:value-of select="@c4" />
          </a>
        </div>
      </td>
      <td class="borderBase" style="width:10%">
        <div style='width:99%;overflow:hidden;'>
          <a href="{$p7}{$p8}/products/projects/tasks.aspx?prjID={@c0}">
            <xsl:value-of select="@c5" />
          </a>
        </div>
      </td>
      <td class="borderBase" style="width:10%">
        <div style='width:99%;overflow:hidden;'>
          <a href="{$p7}{$p8}/products/projects/projectteam.aspx?prjID={@c0}">
            <xsl:value-of select="@c6" />
          </a>
        </div>
      </td>
    </tr>

  </xsl:template>



</xsl:stylesheet>
