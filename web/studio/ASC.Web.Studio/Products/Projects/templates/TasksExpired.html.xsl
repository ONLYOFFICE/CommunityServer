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
  <xsl:param name="p8"/>
  <xsl:param name="p9"/>

  <xsl:key name="g1" match="r" use="@c0"/>
  <xsl:key name="g2" match="r" use="concat(@c0, '|', @c2)"/>
  <xsl:key name="g3" match="r" use="concat(@c0, '|', @c2, '|', @c6)"/>
  
  <xsl:template match="*">

    <div id="reportBody">
      <table class="pm-tablebase" cellspacing="0" cellpadding="6" width="100%" style="display:table; table-layout:fixed;">
        <tbody>
            <tr>
                <!--for width table columns-->
                <td style='width:74%; height:1px;'></td>
                <td style='width:10%; height:1px;'></td>
                <td style='width:15%; height:1px;'></td>
            </tr>
          <xsl:for-each select="//r[generate-id() = generate-id(key('g1', @c0))]" >
            <xsl:variable name="proj" select="@c0"/>
            <tr>
              <td colspan='3'>
                <div style='width:99%;overflow:hidden;'>
                  <span class="menu-item-icon projects" style='color:transparent'>.</span>
                  <a href='{$p8}{$p9}/products/projects/tasks.aspx?prjID={@c0}#sortBy=deadline&amp;sortOrder=ascending&amp;overdue=true' class='report-big-header'>
                    <xsl:value-of select="@c1"/>
                  </a>
                </div>
              </td>
            </tr>

            <xsl:for-each select="key('g1', $proj) [generate-id() = generate-id(key('g2', concat($proj, '|', @c2)))]">
              <xsl:variable name="milestone" select="@c2"/>
              <xsl:variable name="prjMil" select="concat($proj, '|', $milestone)"/>
              <tr>
                <td class='borderBase' style='width:74%;'>
                    <div style='width:95%;overflow:hidden; padding-left:32px;'>
                    <xsl:if test="@c2 = '0'">
                        <a class='report-middle-header' style='padding-left: 22px;' href='{$p8}{$p9}/products/projects/tasks.aspx?prjID={@c0}#sortBy=deadline&amp;sortOrder=ascending&amp;milestone=0&amp;overdue=true'>
                            <xsl:value-of select="$p5"/>
                        </a>
                    </xsl:if>
                    <xsl:if test="@c2 != '0'">
                      <span class="menu-item-icon milestones" style='color:transparent'>.</span>
                      <a class='report-middle-header' href='{$p8}{$p9}/products/projects/tasks.aspx?prjID={@c0}#sortBy=deadline&amp;sortOrder=ascending&amp;milestone={@c2}'>
                        <xsl:value-of select="@c3"/>
                      </a>
                    </xsl:if>
                  </div>
                </td>
                <td class='borderBase' style='width:10%;'>
                  <div style='width:99%;overflow:hidden;font-size:14px;'>
                    <b class='{@c15}'>
                      <xsl:value-of select="@c4"/>
                    </b>
                  </div>
                </td>
                <td class='borderBase' style='width:15%;'></td>
              </tr>

              <xsl:for-each select="key('g2', $prjMil) [generate-id() = generate-id(key('g3', concat($prjMil, '|', @c6)))]">
                <xsl:variable name="task" select="@c6"/>
                <tr>
                  <td class="borderBase" style="'width:74%;padding-left:64px;">
                    <div style='width:99%;overflow:hidden;'>
                      <span class="menu-item-icon tasks" style='color:transparent'>.</span>
                      <a href="{$p8}{$p9}/products/projects/tasks.aspx?prjID={@c0}&amp;ID={@c6}">
                        <xsl:value-of select="@c7"/>
                      </a>
                    </div>
                  </td>
                  <td class="borderBase" style='width:10%;'>
                    <div class="{@c17}" style='width:99%;overflow:hidden;'>
                      <xsl:value-of select="@c10"/>
                    </div>
                  </td>
                  <td class="borderBase" style='width:15%;'>
                    <xsl:for-each select="//r[@c0 = $proj and @c2 = $milestone and @c6 = $task]">
                      <div style='width:99%;overflow:hidden;white-space:nowrap'>
                        <xsl:if test="@c13 = ''">
                          <xsl:value-of select="@c12"/>
                        </xsl:if>
                        <xsl:if test="@c13 != ''">
                          <a href="{$p8}{@c13}">
                            <xsl:value-of select="@c12"/>
                          </a>
                        </xsl:if>
                      </div>
                    </xsl:for-each>
                  </td>
                </tr>
              </xsl:for-each>

            </xsl:for-each>

            <tr>
              <td colspan='3'>
                <br/>
              </td>
            </tr>
          </xsl:for-each>
        </tbody>
      </table>
    </div>
  </xsl:template>

</xsl:stylesheet>