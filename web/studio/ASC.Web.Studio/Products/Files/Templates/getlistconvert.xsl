<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" encoding="utf-8" standalone="yes" indent="yes" omit-xml-declaration="yes" media-type="text/xhtml" />

  <register type="ASC.Web.Files.Resources.FilesCommonResource,ASC.Web.Files" alias="fres" />

  <xsl:template match="entryData">

    <xsl:for-each select="fileTypeBlock">
      <xsl:if test="count(files / file) > 0">
        <div class="">
          <xsl:attribute name="class">
            borderBase cnvrt-file-block cnvrt-file-block-active
            <xsl:choose>
              <xsl:when test="count(files / file) > 1">
                cnvrt-file-block-closed
              </xsl:when>
              <xsl:otherwise>
                cnvrt-file-block-open
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <xsl:if test="count(files / file) > 1">
            <div class="cnvrt-file-block-head">
              <xsl:if test="count(blockFormats / format) > 0">
                <div class="cnvrt-format-title-content">
                  <select class="select-format">
                    <xsl:for-each select="blockFormats/format">
                      <option>
                        <xsl:attribute name="value">
                          <xsl:value-of select="value"/>
                        </xsl:attribute>
                        <xsl:value-of select="name"/>
                      </option>
                    </xsl:for-each>
                  </select>
                  <span class="cnvrt-format-title">
                    <xsl:value-of select="blockFormats/format/name"/>
                  </span>
                </div>
              </xsl:if>
              <div class="cnvrt-status-content"></div>
              <div class="cnvrt-cbx-content">
                <input type="checkbox" checked="checked"/>
              </div>
              <div class="cnvrt-title-content">
                <xsl:value-of select="blockTitle"/>
              </div>
              <xsl:if test="count(blockFormats / format) > 0">
                <div class="cnvrt-tooltip-content">
                  <resource name="fres.ConvertTo"/>
                </div>
              </xsl:if>
            </div>
          </xsl:if>
          <div class="cnvrt-file-block-body">
            <xsl:for-each select="files/file">
              <div class="cnvrt-file-row cnvrt-file-row-active">
                <div class="cnvrt-format-title-content">
                  <xsl:choose>
                    <xsl:when test="count(fileConvertFormats / format) = 1">
                      <span>
                        <xsl:value-of select="fileConvertFormats/format/name"/>
                      </span>
                      <input type="hidden">
                        <xsl:attribute name="file-id">
                          <xsl:value-of select="fileId"/>
                        </xsl:attribute>
                        <xsl:attribute name="value">
                          <xsl:value-of select="fileConvertFormats/format/value"/>
                        </xsl:attribute>
                      </input>
                    </xsl:when>
                    <xsl:otherwise>
                      <select class="select-format">
                        <xsl:attribute name="file-id">
                          <xsl:value-of select="fileId"/>
                        </xsl:attribute>
                        <xsl:for-each select="fileConvertFormats/format">
                          <option>
                            <xsl:attribute name="value">
                              <xsl:value-of select="value"/>
                            </xsl:attribute>
                            <xsl:value-of select="name"/>
                          </option>
                        </xsl:for-each>
                      </select>
                      <span class="cnvrt-format-title">
                        <xsl:value-of select="fileConvertFormats/format/name"/>
                      </span>
                    </xsl:otherwise>
                  </xsl:choose>
                </div>
                <div class="cnvrt-cbx-content">
                  <input type="checkbox" checked="checked"/>
                </div>
                <div>
                  <xsl:attribute name="class">
                    cnvrt-format-icon-content <xsl:value-of select="fileCssClass"/>
                  </xsl:attribute>
                </div>
                <div class="cnvrt-title-content">
                  <xsl:attribute name="title">
                    <xsl:value-of select="fileTitle"/>
                  </xsl:attribute>
                  <xsl:value-of select="fileTitle"/>
                </div>
                <xsl:if test="count(fileConvertFormats / format) > 1">
                  <div class="cnvrt-tooltip-content">
                    <resource name="fres.ConvertTo"/>
                  </div>
                </xsl:if>
              </div>
            </xsl:for-each>
          </div>
        </div>
      </xsl:if>
    </xsl:for-each>

  </xsl:template>

</xsl:stylesheet>