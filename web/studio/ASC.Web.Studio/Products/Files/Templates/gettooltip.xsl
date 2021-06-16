<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" encoding="utf-8" standalone="yes" indent="yes" omit-xml-declaration="yes" media-type="text/xhtml" />

  <register type="ASC.Web.Files.Resources.FilesCommonResource,ASC.Web.Files" alias="FilesCommonResource" />

  <xsl:template match="entryTooltip">
    <table cellpadding="0" cellspacing="0">
      <tbody>
        <xsl:if test="thumbnail != 'false'">
          <tr>
            <td rowspan="6">
              <img alt="">
                <xsl:attribute name="src">
                  <xsl:value-of select="thumbnail" />
                </xsl:attribute>
              </img>
            </td>
          </tr>
        </xsl:if>
        <tr>
          <td>
            <xsl:choose>
              <xsl:when test="type = 'file'">
                <resource name="FilesCommonResource.DocumentName" />:
              </xsl:when>

              <xsl:when test="type = 'folder'">
                <resource name="FilesCommonResource.FolderName" />:
              </xsl:when>
            </xsl:choose>
          </td>
          <td>
            <xsl:value-of select="title" />
          </td>
        </tr>
        <xsl:choose>
          <xsl:when test="error = 'true'">
            <tr>
              <td>
                <resource name="FilesCommonResource.ErrorEntry" />:
              </td>
              <td>
                <xsl:value-of select="error" />
              </td>
            </tr>
          </xsl:when>

          <xsl:otherwise>
            <tr>
              <td>
                <resource name="FilesCommonResource.Author" />:
              </td>
              <td>
                <span class="userLink">
                  <xsl:value-of select="create_by" />
                </span>
              </td>
            </tr>
            <tr>
              <td>
                <xsl:choose>
                  <xsl:when test="date_type = 'upload'">
                    <resource name="FilesCommonResource.TitleUploaded" />:
                  </xsl:when>
                  <xsl:when test="date_type = 'update'">
                    <resource name="FilesCommonResource.TitleModified" />:
                  </xsl:when>
                  <xsl:when test="date_type = 'remove'">
                    <resource name="FilesCommonResource.TitleRemoved" />:
                  </xsl:when>
                  <xsl:when test="date_type = 'create'">
                    <resource name="FilesCommonResource.TitleCreated" />:
                  </xsl:when>
                </xsl:choose>
              </td>
              <td>
                <xsl:value-of select="modified_on" />
              </td>
            </tr>

            <xsl:choose>
              <xsl:when test="type = 'file'">
                <tr>
                  <td>
                    <resource name="FilesCommonResource.Size" />:
                  </td>
                  <td>
                    <xsl:value-of select="length_string" />
                  </td>
                </tr>

                <xsl:if test="comment and comment != ''">
                  <tr>
                    <td>
                      <resource name="FilesCommonResource.Comment" />:
                    </td>
                    <td>
                      <xsl:value-of select="comment" />
                    </td>
                  </tr>
                </xsl:if>
              </xsl:when>

              <xsl:when test="type = 'folder' and (not(provider_key) or provider_key = '')">
                <tr>
                  <td>
                    <resource name="FilesCommonResource.TitleFiles" />:
                  </td>
                  <td>
                    <xsl:value-of select="total_files" />
                  </td>
                </tr>
                <tr>
                  <td>
                    <resource name="FilesCommonResource.TitleSubfolders" />:
                  </td>
                  <td>
                    <xsl:value-of select="total_sub_folder" />
                  </td>
                </tr>
              </xsl:when>
            </xsl:choose>

          </xsl:otherwise>
        </xsl:choose>
      </tbody>
    </table>
  </xsl:template>

</xsl:stylesheet>