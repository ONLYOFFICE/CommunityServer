<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="http://www.w3.org/1999/xhtml">
  <xsl:output method="html" />

  <xsl:param name="p0"/>
  <xsl:param name="p1"/>
  <xsl:param name="p2"/>
  <xsl:param name="p3"/>

  <xsl:key name="projects" match="r" use="@c0" />

  <xsl:template match="*">

    <div id="reportBody">
      <table class="pm-tablebase" cellspacing="0" cellpadding="6" width="100%">
        <tbody>
            <tr>
                <!--for width table columns-->
                <td style="width: 89%; height:1px;"></td>
                <td style="width: 10%; height:1px;"></td>
            </tr>
          <xsl:apply-templates select="r[generate-id(.)=generate-id(key('projects', @c0))]"/>
        </tbody>
      </table>
    </div>
  </xsl:template>

  <xsl:template match="r">

    <tr>
      <td colspan='2' style='padding-left:15px; padding-right:15px;'>
          <div style='width:99%;overflow:hidden;'>
              <span class="menu-item-icon projects" style='color:transparent'>.</span>
              <a class="report-middle-header" href='{$p3}/products/projects/milestones.aspx?prjID={@c0}#sortBy=deadline&amp;sortOrder=ascending&amp;overdue=true'>
                <xsl:value-of select="@c1"/>
              </a>
        </div>
      </td>
    </tr>

    <xsl:apply-templates mode="next" select="key('projects', @c0)"/>

    <tr>
      <td colspan='2'>
        <br/>
      </td>
    </tr>

  </xsl:template>

  <xsl:template match="r" mode="next">
    <tr>
      <td class='borderBase' style='padding-left:32px;'>
          <div style='width:99%;overflow:hidden;'>
              <span class="menu-item-icon milestones" style='color:transparent'>.</span>
              <a class="report-small-header" href='{$p3}/products/projects/tasks.aspx?prjID={@c0}#sortBy=deadline&amp;sortOrder=ascending&amp;milestone={@c2}'>
                <xsl:value-of select="@c3"/>
              </a>
        </div>
      </td>
      <td class='borderBase' style='text-align: right;padding-right:15px;'>
        <div class='red-text' style='width:80px;overflow:hidden;'>
          <xsl:value-of select="@c4"/>
        </div>
      </td>
    </tr>
  </xsl:template>

</xsl:stylesheet>