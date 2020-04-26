<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:output method="xml" version="1.0" encoding="utf-8" indent="yes"/>

  <!-- default value of cur_lang parameter -->
  <xsl:param name="cur_lang">ru</xsl:param>

  <xsl:template match="/">
    <xsl:processing-instruction name="xml-stylesheet">
      <xsl:text>href="vcrtalalaevconclusion.xsl" type="text/xsl"</xsl:text>
    </xsl:processing-instruction>

    <vcrtalalaev>

      <xsl:apply-templates select="vcr_two_dim/row">
        <xsl:sort select="@key"/>
      </xsl:apply-templates>

    </vcrtalalaev>

  </xsl:template>

  <xsl:template match="row">

    <xsl:apply-templates select="entry">
      <xsl:sort select="@key"/>
    </xsl:apply-templates>

  </xsl:template>


  <xsl:template match="entry">
    <entry>
      <xsl:apply-templates select="lang"/>
    </entry>
  </xsl:template>

  <xsl:template match="lang">
    <xsl:apply-templates select="user"></xsl:apply-templates>
    <xsl:apply-templates select="prof"></xsl:apply-templates>
  </xsl:template>


  <xsl:template match="user | prof">
    <xsl:if test="../@langid=$cur_lang">
        <xsl:attribute name="key">
          <xsl:value-of select="concat(../../../@key, ../../@key)"/>
        </xsl:attribute>

        <xsl:attribute name="state_color">
          <xsl:value-of select="../../@state_color"/>
        </xsl:attribute>

        <xsl:element name="{name(.)}">
        
          <xsl:for-each select="para">
            <xsl:if test="position()!=1">
              <!-- если это не первый абзац текста, вставляем знак перевода строки -->
              <xsl:text>&#xa;</xsl:text>
            </xsl:if>

            <xsl:value-of select="."/>
          </xsl:for-each>
        </xsl:element>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>