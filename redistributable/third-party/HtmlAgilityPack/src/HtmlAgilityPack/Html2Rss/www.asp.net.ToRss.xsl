<?xml version="1.0" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
<xsl:param name="url"/>
<xsl:param name="fromCache"/>
<xsl:param name="requestDuration"/>
<xsl:template match="/">
	<rss version="0.91">
		<channel>
			<title>ASP.Net articles scrap RSS feed</title>
			<link><xsl:value-of select="$url"/></link>
			<xsl:for-each select="//a[@href and @target='_new']">
			<item>
				<title><xsl:value-of select="./text()" /></title>
				<link><xsl:value-of select="./@href" /></link>
				<description><xsl:value-of select="../div/text()" /></description>
			</item>
			</xsl:for-each>
		</channel>
	</rss>
</xsl:template>
</xsl:stylesheet>