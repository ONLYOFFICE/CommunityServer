<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" encoding="utf-8" standalone="yes" indent="yes" omit-xml-declaration="yes" media-type="text/xhtml" />

  <register type="ASC.Web.Files.Resources.FilesCommonResource,ASC.Web.Files" alias="fres" />

  <xsl:template match="entryList">
    <xsl:for-each select="entry">

      <xsl:if test="@*='folder'">

        <li>
          <xsl:attribute name="class">
            file-row folder-row
            <xsl:if test="error != ''">
              error-entry
            </xsl:if>
          </xsl:attribute>

          <xsl:attribute name="data-id">folder_<xsl:value-of select="id" /></xsl:attribute>

          <div class="thumb-folder">
            <xsl:attribute name="title">
              <xsl:value-of select="title" />
            </xsl:attribute>
            <xsl:if test="provider_key != ''">
              <div>
                <xsl:attribute name="class">
                  provider-key
                  <xsl:value-of select="provider_key" />
                </xsl:attribute>
              </div>
            </xsl:if>
          </div>

          <div class="entry-info">
            <div class="entry-title">
              <div class="name">
                <a>
                  <xsl:attribute name="title">
                    <xsl:value-of select="title" />
                  </xsl:attribute>
                  <xsl:value-of select="title" />
                </a>
              </div>
            </div>

            <input type="hidden" name="entry_data">
              <xsl:attribute name="data-id">
                <xsl:value-of select="id" />
              </xsl:attribute>
              <xsl:attribute name="value">
                {
                "entryType": "<xsl:value-of select="@*" />",
                "access": <xsl:value-of select="access" />,
                "create_by_id": "<xsl:value-of select="create_by_id" />",
                "create_on": "<xsl:value-of select="create_on" />",
                <!--"id": "<xsl:value-of select="id" />",-->
                "modified_on": "<xsl:value-of select="modified_on" />",
                "shared": <xsl:value-of select="shared" />,
                "title": "<xsl:value-of select="title" />",
                "isnew": <xsl:value-of select="isnew" />,
                "provider_key": "<xsl:value-of select="provider_key" />",
                "provider_id": "<xsl:value-of select="provider_id" />",
                "error": "<xsl:value-of select="error" />"
                <!--create_by: "<xsl:value-of select="create_by" />",    encode-->
                <!--modified_by: "<xsl:value-of select="modified_by" />",    encode-->
                <!--total_files: <xsl:value-of select="total_files" />,    dynamic-->
                <!--total_sub_folder: <xsl:value-of select="total_sub_folder" />,    dynamic-->
                }
              </xsl:attribute>
            </input>
          </div>
        </li>

      </xsl:if>

      <xsl:if test="@*='file'">

        <li>
          <xsl:attribute name="class">
            file-row
            <xsl:if test="error != ''">
              error-entry
            </xsl:if>
          </xsl:attribute>

          <xsl:attribute name="data-id">file_<xsl:value-of select="id" /></xsl:attribute>
          <div class="thumb-file">
            <xsl:attribute name="title">
              <xsl:value-of select="title" />
            </xsl:attribute>
          </div>

          <div class="entry-info">
            <div class="entry-title">
              <div class="name">
                <a>
                  <xsl:attribute name="title">
                    <xsl:value-of select="title" />
                  </xsl:attribute>
                  <xsl:value-of select="title" />
                </a>
              </div>
            </div>

            <input type="hidden" name="entry_data">
              <xsl:attribute name="data-id">
                <xsl:value-of select="id" />
              </xsl:attribute>
              <xsl:attribute name="value">
                {
                "entryType": "<xsl:value-of select="@*" />",
                "access": <xsl:value-of select="access" />,
                "create_by_id": "<xsl:value-of select="create_by_id" />",
                "create_on": "<xsl:value-of select="create_on" />",
                <!--"id": "<xsl:value-of select="id" />",-->
                "modified_on": "<xsl:value-of select="modified_on" />",
                "shared": <xsl:value-of select="shared" />,
                "title": "<xsl:value-of select="title" />",
                "content_length": "<xsl:value-of select="content_length" />",
                "file_status": "<xsl:value-of select="file_status" />",
                "version": <xsl:value-of select="version" />,
                "version_group": <xsl:value-of select="version_group" />,
                "provider_key": "<xsl:value-of select="provider_key" />",
                "error": "<xsl:value-of select="error" />"
                <!--create_by: "<xsl:value-of select="create_by" />",    encode-->
                <!--modified_by: "<xsl:value-of select="modified_by" />",    encode-->
                }
              </xsl:attribute>
            </input>
          </div>
        </li>

      </xsl:if>

    </xsl:for-each>
  </xsl:template>

</xsl:stylesheet>