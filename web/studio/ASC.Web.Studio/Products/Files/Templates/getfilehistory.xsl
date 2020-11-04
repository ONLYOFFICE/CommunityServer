<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" encoding="utf-8" standalone="yes" indent="yes" omit-xml-declaration="yes" media-type="text/xhtml" />

  <register type="ASC.Web.Files.Resources.FilesCommonResource,ASC.Web.Files" alias="fres" />

  <xsl:template match="fileList">
    <div id="contentVersions">
      <table class="not-preview" cellspacing="0" cellpadding="0" border="0">
        <thead>
          <tr class="versions-title">
            <th colspan="7">
              <resource name="fres.TitleVersionHistory" />
            </th>
            <th>
              <span class="version-close"></span>
            </th>
          </tr>
        </thead>
        <xsl:for-each select="entry">
          <tr class="version-row" >
            <xsl:attribute name="data-version-group">
              <xsl:value-of select="version_group" />
            </xsl:attribute>
            <xsl:attribute name="data-version">
              <xsl:value-of select="version" />
            </xsl:attribute>
            <td class="version-num gray-text">
              <span>
                <xsl:value-of select="version_group" />.
              </span>
              <div class="version-complete">
                <xsl:attribute name="title">
                  <resource name="fres.TitleVersionComplete" />
                </xsl:attribute>
              </div>
              <div class="version-continue">
                <xsl:attribute name="title">
                  <resource name="fres.TitleVersionContinue" />
                </xsl:attribute>
              </div>
            </td>
            <td class="version-date">
              <xsl:value-of select="modified_on" />
            </td>
            <td class="version-sublist">
              <span></span>
            </td>
            <td class="version-author" >
              <span class="userLink">
                <xsl:attribute name="id">version_<xsl:value-of select="id" />_<xsl:value-of select="version" /></xsl:attribute>
                <xsl:attribute name="data-uid">
                  <xsl:value-of select="modified_by_id" />
                </xsl:attribute>
                <xsl:value-of select="modified_by" />
              </span>
            </td>
            <td class="version-comment" >
              <xsl:if test="comment and comment != ''">
                <xsl:attribute name="title">
                  <xsl:value-of select="comment"/>
                </xsl:attribute>
              </xsl:if>
              <xsl:attribute name="data-comment">
                <xsl:value-of select="comment" />
              </xsl:attribute>
              <div class="version-comment-fix">
                <xsl:value-of select="comment"/>
              </div>
            </td>
            <td class="version-operation">
              <div class="version-download">
                <xsl:attribute name="title">
                  <resource name="fres.ButtonDownload" />
                </xsl:attribute>
              </div>
              <div class="version-preview">
                <xsl:attribute name="title">
                  <resource name="fres.OpenFile" />
                </xsl:attribute>
              </div>
              <div class="version-comment-edit">
                <xsl:attribute name="title">
                  <resource name="fres.EnterComment" />
                </xsl:attribute>
              </div>
            </td>
            <td class="version-size">
              <xsl:value-of select="content_length_string" />
            </td>
            <td class="version-operation version-restore">
              <span>
                <xsl:attribute name="title">
                  <resource name="fres.MakeCurrent" />
                </xsl:attribute>
                <resource name="fres.MakeCurrent" />
              </span>
            </td>
          </tr>
        </xsl:for-each>
      </table>
    </div>
  </xsl:template>

</xsl:stylesheet>