<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" encoding="utf-8" standalone="yes" indent="yes" omit-xml-declaration="yes" media-type="text/xhtml" />

  <register type="ASC.Web.Files.Resources.FilesCommonResource,ASC.Web.Files" alias="fres" />

  <xsl:template match="DataToImportList">
    <div class="borderBase import-data-table" >
      <table class="fl-tablebase sortable">
        <thead>
          <tr>
            <td class="import-data-check unsortable">
              <input name="all_checked_document" type="checkbox" />
            </td>
            <td>
              <span class="baseLinkAction">
                <resource name="fres.DocumentName" />
              </span>
              <span id="sorttable_sortfwdind"> ▴</span>
            </td>
            <td class="import-data-author">
              <span class="baseLinkAction">
                <resource name="fres.Author" />
              </span>
            </td>
            <td class="import-data-date">
              <span class="baseLinkAction">
                <resource name="fres.CreatingDate" />
              </span>
            </td>
          </tr>
        </thead>
        <tbody>
          <xsl:for-each select="entry">
            <tr>
              <xsl:if test="position() mod 2">
                <xsl:attribute name="class">tintMedium</xsl:attribute>
              </xsl:if>
              <td class="borderBase import-data-check">
                <input type="checkbox" name="checked_document">
                  <xsl:attribute name="value">
                    <xsl:value-of select="content_link" />
                  </xsl:attribute>
                </input>
              </td>
              <td class="borderBase">
                <div class="import-title">
                  <xsl:attribute name="title">
                    <xsl:value-of select="title" />
                  </xsl:attribute>
                  <xsl:value-of select="title" />
                </div>
              </td>
              <td class="borderBase import-data-author">
                <div class="import-author">
                  <xsl:attribute name="title">
                    <xsl:value-of select="create_by" />
                  </xsl:attribute>
                  <xsl:value-of select="create_by" />
                </div>
              </td>
              <td class="borderBase import-data-date">
                <xsl:value-of select="create_on" />
              </td>
            </tr>
          </xsl:for-each>
        </tbody>
      </table>
    </div>
  </xsl:template>

</xsl:stylesheet>