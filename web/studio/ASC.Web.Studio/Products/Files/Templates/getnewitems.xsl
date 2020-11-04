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
              <xsl:attribute name="data-entryType">folder</xsl:attribute>
              <xsl:attribute name="data-access">
                <xsl:value-of select="access" />
              </xsl:attribute>
              <xsl:attribute name="data-shared">
                <xsl:value-of select="shared" />
              </xsl:attribute>
              <xsl:attribute name="data-create_by_id">
                <xsl:value-of select="create_by_id" />
              </xsl:attribute>
              <xsl:attribute name="data-isnew">
                <xsl:value-of select="isnew" />
              </xsl:attribute>
              <xsl:attribute name="data-provider_key">
                <xsl:value-of select="provider_key" />
              </xsl:attribute>
              <xsl:attribute name="data-provider_id">
                <xsl:value-of select="provider_id" />
              </xsl:attribute>
              <xsl:attribute name="data-error">
                <xsl:value-of select="error" />
              </xsl:attribute>
              <xsl:attribute name="data-title">
                <xsl:value-of select="title" />
              </xsl:attribute>
              <xsl:attribute name="data-create_on">
                <xsl:value-of select="create_on" />
              </xsl:attribute>
              <xsl:attribute name="data-modified_on">
                <xsl:value-of select="modified_on" />
              </xsl:attribute>
              <xsl:attribute name="data-create_by">
                <xsl:value-of select="create_by" />
              </xsl:attribute>
              <xsl:attribute name="data-modified_by">
                <xsl:value-of select="modified_by" />
              </xsl:attribute>
              <xsl:attribute name="data-comment">
                <xsl:value-of select="comment" />
              </xsl:attribute>
              <xsl:attribute name="data-folder_url">
                <xsl:value-of select="folder_url" />
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
              <xsl:attribute name="data-entryType">file</xsl:attribute>
              <xsl:attribute name="data-access">
                <xsl:value-of select="access" />
              </xsl:attribute>
              <xsl:attribute name="data-shared">
                <xsl:value-of select="shared" />
              </xsl:attribute>
              <xsl:attribute name="data-create_by_id">
                <xsl:value-of select="create_by_id" />
              </xsl:attribute>
              <xsl:attribute name="data-isnew">
                <xsl:value-of select="isnew" />
              </xsl:attribute>
              <xsl:attribute name="data-provider_key">
                <xsl:value-of select="provider_key" />
              </xsl:attribute>
              <xsl:attribute name="data-provider_id">
                <xsl:value-of select="provider_id" />
              </xsl:attribute>
              <xsl:attribute name="data-error">
                <xsl:value-of select="error" />
              </xsl:attribute>
              <xsl:attribute name="data-title">
                <xsl:value-of select="title" />
              </xsl:attribute>
              <xsl:attribute name="data-create_on">
                <xsl:value-of select="create_on" />
              </xsl:attribute>
              <xsl:attribute name="data-modified_on">
                <xsl:value-of select="modified_on" />
              </xsl:attribute>
              <xsl:attribute name="data-file_status">
                <xsl:value-of select="file_status" />
              </xsl:attribute>
              <xsl:attribute name="data-version_group">
                <xsl:value-of select="version_group" />
              </xsl:attribute>
              <xsl:attribute name="data-content_length">
                <xsl:value-of select="content_length" />
              </xsl:attribute>
              <xsl:attribute name="data-content_length_string">
                <xsl:value-of select="content_length_string" />
              </xsl:attribute>
              <xsl:attribute name="data-version">
                <xsl:value-of select="version" />
              </xsl:attribute>
              <xsl:attribute name="data-create_by">
                <xsl:value-of select="create_by" />
              </xsl:attribute>
              <xsl:attribute name="data-modified_by">
                <xsl:value-of select="modified_by" />
              </xsl:attribute>
              <xsl:attribute name="data-comment">
                <xsl:value-of select="comment" />
              </xsl:attribute>
            </input>
          </div>
        </li>

      </xsl:if>

    </xsl:for-each>
  </xsl:template>

</xsl:stylesheet>