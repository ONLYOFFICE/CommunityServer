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
  <xsl:key name="userPrj" match="r" use="concat(@c0, '|', @c1)"/>
  <xsl:key name="userPrjTask" match="r" use="concat(@c0, '|', @c1, '|', @c3)"/>
  
  <xsl:template match="*">

    <div id="reportBody">
      <table class="pm-tablebase no-sorted" cellspacing="0" cellpadding="6" width="100%">
        <thead>
          <tr>
            <td style="width:69%;text-align:left;">
              <span>
                <xsl:attribute name="title">
                  <xsl:value-of select="$p3"/>
                </xsl:attribute>
                <xsl:value-of select="$p0"/>
              </span>
            </td>
            <td style="width:15%;text-align: right;white-space: nowrap;">
              <span>
                <xsl:attribute name="title">
                  <xsl:value-of select="$p3"/>
                </xsl:attribute>
                <xsl:value-of select="$p1"/>
              </span>
            </td>
            <xsl:if test='($p8=-1)'>
            <td style="width:15%;text-align: right;white-space: nowrap;">
              <span>
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
              <td class='borderBase'>
              </td>
              <td class="borderBase" style="text-align: right;">
                <div style='overflow:hidden;font-size:16px;font-weight:bold;'>
                  <xsl:call-template name="GetTime">
                    <xsl:with-param name="time" select="sum(//@c5)"/>
                  </xsl:call-template>
                </div>
              </td>
              <xsl:if test='($p8=-1)'>
                <td class="borderBase" style="text-align: right;">
                  <div style='overflow:hidden;font-size:16px;font-weight:bold;'>
                    <xsl:call-template name="GetTime">
                      <xsl:with-param name="time" select="sum(//@c6)"/>
                    </xsl:call-template>
                  </div>
                </td>
              </xsl:if>
            </tr>
        </tbody>
      </table>
    </div>
  </xsl:template>

  <xsl:template match="r">
    <xsl:variable name="user" select="@c0"/>
    <tr>
      <td style='width:69%;padding-left:8px; padding-right:8px;font-weight:bold;font-size:16px;'>
        <div style='width:99%;overflow:hidden;'>
          <a href='{$p4}{@c9}' class='report-big-header'>
            <xsl:value-of select="@c8"/>
          </a>
        </div>
      </td>
      <td  style='width:15%;text-align: right;'>
        <div style='width:99%;overflow:hidden;font-weight:bold;font-size:16px;'>
          <xsl:call-template name="GetTime">
            <xsl:with-param name="time" select="sum(key('users', @c0)/@c5)"/>
          </xsl:call-template>
        </div>
      </td>
      <xsl:if test='($p8=-1)'>
        <td  style='width:15%;text-align: right;'>
          <div style='width:99%;overflow:hidden;font-weight:bold;font-size:16px;'>
            <xsl:call-template name="GetTime">
              <xsl:with-param name="time" select="sum(key('users', @c0)/@c6)"/>
            </xsl:call-template>
          </div>
        </td>
      </xsl:if>
    </tr>

    <xsl:apply-templates mode="proj" select="key('users', $user) [generate-id() = generate-id(key('userPrj', concat($user, '|', @c1)))]">
      <xsl:sort select="@c2" data-type="text"/>
    </xsl:apply-templates>

  </xsl:template>
  
  <xsl:template match="r" mode="proj">
    <xsl:variable name="user" select="@c0"/>
    <xsl:variable name="prj" select="@c1"/>
    <tr>
      <td class='borderBase' style='width:69%;padding-left:8px;'>
        <span class="menu-item-icon projects" style='color:transparent'>.</span>
        <div style='width:96%;overflow:hidden;'>
          <a class='report-middle-header' href='{$p4}{$p5}/products/projects/projects.aspx?prjID={@c1}'>
              <xsl:value-of select="@c2"/>
          </a>
        </div>
      </td>
      <td class="borderBase" style='width:15%;text-align: right;'>
        <div style='width:99%;overflow:hidden;font-size:14px;'>
          <xsl:call-template name="GetTime">
            <xsl:with-param name="time" select="sum(key('userPrj', concat($user, '|', @c1))/@c5)"/>
          </xsl:call-template>
        </div>
      </td>
      <xsl:if test='($p8=-1)'>
        <td class="borderBase" style='width:15%;text-align: right;'>
          <div style='width:99%;overflow:hidden;font-size:14px;'>
            <xsl:call-template name="GetTime">
              <xsl:with-param name="time" select="sum(key('userPrj', concat($user, '|', @c1))/@c6)"/>
            </xsl:call-template>
          </div>
        </td>
      </xsl:if>
    </tr>

    <xsl:apply-templates mode="task" select="key('userPrj', concat($user, '|', $prj))[generate-id() = generate-id(key('userPrjTask', concat($user, '|', $prj, '|', @c3)))]">
    </xsl:apply-templates>

    <tr>
      <td colspan='2'>
        <br/>
      </td>
    </tr>
  </xsl:template>
  
  <xsl:template match="r" mode="task">
    <tr>
      <td class='borderBase' style='width:69%;padding-left:32px;'>
        <span class='menu-item-icon tasks' style='color:transparent'>.</span>
        <div style='width:95%;overflow:hidden;font-size:12px;'>
          <a href='{$p4}{$p5}/products/projects/tasks.aspx?prjID={@c1}&amp;ID={@c3}'>
              <xsl:value-of select="@c4"/>
          </a>
        </div>
      </td>
      <td class="borderBase" style='width:15%;text-align: right;'>
        <div style='width:99%;overflow:hidden;font-size:12px;'>
          <xsl:call-template name="GetTime">
            <xsl:with-param name="time" select="sum(key('userPrjTask',  concat(@c0, '|', @c1, '|', @c3))/@c5)"/>
          </xsl:call-template>
        </div>
      </td>
      <xsl:if test='($p8=-1)'>
        <td class="borderBase" style='width:15%;text-align: right;'>
          <div style='width:99%;overflow:hidden;font-size:12px;'>
            <xsl:call-template name="GetTime">
              <xsl:with-param name="time" select="sum(key('userPrjTask',  concat(@c0, '|', @c1, '|', @c3))/@c6)"/>
            </xsl:call-template>
          </div>
        </td>
      </xsl:if>
    </tr>
  </xsl:template>
  
  <xsl:template name="GetTime">
    <xsl:param name="time"/>
    <xsl:variable name="hours" select="floor($time)"/>
    <xsl:variable name="minutes" select="round((($time)-($hours))*60)"/>
    <xsl:if test='($time=0.00)'>
      -
    </xsl:if>
    <xsl:if test='($time!=0.00)'>
      <xsl:value-of select="concat($hours,$p6,' ',format-number($minutes,'00'),$p7)"/>
    </xsl:if>

  </xsl:template>
  
</xsl:stylesheet>