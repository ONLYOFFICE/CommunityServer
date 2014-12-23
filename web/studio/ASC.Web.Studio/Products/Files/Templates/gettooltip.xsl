<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" encoding="utf-8" standalone="yes" indent="yes" omit-xml-declaration="yes" media-type="text/xhtml" />

  <register type="ASC.Web.Files.Resources.FilesCommonResource,ASC.Web.Files" alias="fres" />

  <xsl:template match="entryTooltip">
    <dl>

      <dt>
        <xsl:choose>
          <xsl:when test="type = 'file'">
            <resource name="fres.DocumentName" />:
          </xsl:when>
          <xsl:when test="type = 'folder'">
            <resource name="fres.FolderName" />:
          </xsl:when>
        </xsl:choose>
      </dt>
      <dd>
        <xsl:value-of select="title" />
      </dd>

      <xsl:choose>
        <xsl:when test="error = 'true'">
          <dt>
            <resource name="fres.ErrorEntry" />:
          </dt>
          <dd>
            <xsl:value-of select="error" />
          </dd>
        </xsl:when>
        <xsl:otherwise>

          <dt>
            <resource name="fres.Author" />:
          </dt>
          <dd>
            <span class="userLink">
              <xsl:value-of select="create_by" />
            </span>
          </dd>
          <dt>
            <xsl:choose>
              <xsl:when test="date_type = 'upload'">
                <resource name="fres.TitleUploaded" />:
              </xsl:when>
              <xsl:when test="date_type = 'update'">
                <resource name="fres.TitleModified" />:
              </xsl:when>
              <xsl:when test="date_type = 'remove'">
                <resource name="fres.TitleRemoved" />:
              </xsl:when>
              <xsl:when test="date_type = 'create'">
                <resource name="fres.TitleCreated" />:
              </xsl:when>
            </xsl:choose>
          </dt>
          <dd>
            <xsl:value-of select="modified_on" />
          </dd>

          <xsl:choose>
            <xsl:when test="type = 'file'">

              <dt>
                <resource name="fres.Size" />:
              </dt>
              <dd>
                <xsl:value-of select="length" />
              </dd>

            </xsl:when>
            <xsl:when test="type = 'folder' and (not(provider_key) or provider_key = '')">
              <dt>
                <resource name="fres.TitleFiles" />:
              </dt>
              <dd>
                <xsl:value-of select="total_files" />
              </dd>
              <dt>
                <resource name="fres.TitleSubfolders" />:
              </dt>
              <dd>
                <xsl:value-of select="total_sub_folder" />
              </dd>
            </xsl:when>
          </xsl:choose>

        </xsl:otherwise>
      </xsl:choose>

    </dl>
  </xsl:template>

</xsl:stylesheet>