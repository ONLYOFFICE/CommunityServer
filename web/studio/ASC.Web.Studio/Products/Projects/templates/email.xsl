<?xml version="1.0"?>
<xsl:stylesheet version="2.0"
  xmlns:xhtml="http://www.w3.org/1999/xhtml"
  xmlns="http://www.w3.org/1999/xhtml"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  exclude-result-prefixes="xhtml xsl xs">

  <xsl:output
    method="html"
    version="1.0"
    encoding="utf-8"
    doctype-public="-//W3C//DTD XHTML 1.1//EN"
    doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"
    indent="yes"/>

  <xsl:param name="p0"/>
  <xsl:param name="p1"/>
  <xsl:param name="logo"/>

  <xsl:template match="/">
    <html>
      <head>
        <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
        <meta name="GENERATOR" content="Teamlab Auto Reports"/>
        <style>
            .borderBase {
            border:solid 1px #d1d1d1;
            }

            table.sortable, table.pm-tablebase {
            border-collapse: collapse;
            width: 100%;
            table-layout: fixed;
            }

            table.sortable thead .report-tableColumnHeader {
            color: #333;
            font-size: 12px;
            }

            .sortable tbody td,
            .pm-tablebase tbody td {
            text-align: left;
            border-left: none;
            border-right: none;
            vertical-align: middle;
            }

            /*--text--*/
            .gray-text {
            color: #83888d !important;
            }
            .red-text {
            color: #bf3a3c !important;
            }

            /*---headers----*/

            a.report-big-header,
            a.report-middle-header,
            a.report-small-header,
            a.report-big-header:visited,
            a.report-middle-header:visited,
            a.report-small-header:visited,
            a.report-big-header:hover,
            a.report-middle-header:hover,
            a.report-small-header:hover,
            a.report-big-header:active,
            a.report-middle-header:active,
            a.report-small-header:active   {
            font-weight: bold;
            font-family: Arial;
            color: #333;
            text-decoration:none;
            }

            a.report-big-header:hover,
            a.report-middle-header:hover,
            a.report-small-header:hover {
            text-decoration:undeline;
            }

            a.report-big-header {
            font-size: 16px;
            }
            a.report-middle-header {
            font-size: 14px;
            }
            /*---*/
            #reportBody tr td a:link,
            #reportBody tr td a:visited,
            #reportBody tr td a:hover,
            #reportBody tr td a:active {
            color: #333;
            outline: none;
            }
            #reportBody tr td a:link,
            #reportBody tr td a:visited,
            #reportBody tr td a:active {
            text-decoration: none;
            }

            #reportBody tr td a:hover {
            text-decoration: underline;
            }
            /*--*/
            .noContentBlock
            {
            color: #373737;
            font-size: 14px;
            font-weight: bold;
            margin: 5px 0 11px;
            padding: 100px;
            text-align: center;
            }
        </style>
      </head>
      <body>
        <div style="background-color:#31a3d3; height:50px;">
              <img style="float:left;" alt="TeamLab Office" src="{$logo}" />
        </div>
        <div style="margin:0px 40px;">
          <xsl:copy>
            <xsl:apply-templates select="@*|node()"/>
          </xsl:copy>
        </div>
        <div style="margin:30px 0 0 40px;">
            <a style="color:#3d4a6b; font-size: 11px;">
              <xsl:attribute name="href">
                <xsl:value-of select="$p0"/>
              </xsl:attribute>
              <xsl:value-of select="$p1"/>
            </a>
        </div>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>