<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" encoding="utf-8" standalone="yes" indent="yes" omit-xml-declaration="yes" media-type="text/xhtml" />

  <xsl:template match="folderList">
    <xsl:for-each select="entry">
      <li>
        <xsl:attribute name="class">
          tree-node jstree-closed
          <xsl:if test="access = '2'">
            access-read
          </xsl:if>
          <xsl:if test="total_sub_folder = 0 and (not(provider_key) or provider_key = '')">
            jstree-empty
          </xsl:if>
          <xsl:if test="provider_key != ''">
            third-party-entry
          </xsl:if>
        </xsl:attribute>
        <xsl:attribute name="data-id"><xsl:value-of select="id" /></xsl:attribute>
        <xsl:if test="folder_url != ''">
          <xsl:attribute name="data-href">
            <xsl:value-of select="folder_url" />
          </xsl:attribute>
        </xsl:if>
        <div class="jstree-wholerow">&#160;</div>
        <span class="jstree-icon jstree-expander" > </span>
        <a>
          <xsl:attribute name="title">
            <xsl:value-of select="title" />
          </xsl:attribute>
          <xsl:attribute name="data-id"><xsl:value-of select="id" /></xsl:attribute>
          <span>
            <xsl:attribute name="class">
              jstree-icon
              <xsl:if test="provider_key != ''">
                <xsl:value-of select="provider_key" />
              </xsl:if>
            </xsl:attribute>
          </span>
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
          <xsl:value-of select="title" />
        </a>
      </li>
    </xsl:for-each>
  </xsl:template>

</xsl:stylesheet>