<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" version="1.0" encoding="utf-8" indent="yes"/>

  <!-- default value of cur_lang parameter -->
  <xsl:param name="cur_lang">ru</xsl:param>

  <xsl:template match="/">

    <root>
      <!-- 
    Microsoft ResX Schema 
    
    Version 2.0
    
    The primary goals of this format is to allow a simple XML format 
    that is mostly human readable. The generation and parsing of the 
    various data types are done through the TypeConverter classes 
    associated with the data types.
    
    Example:
    
    ... ado.net/XML headers & schema ...
    <resheader name="resmimetype">text/microsoft-resx</resheader>
    <resheader name="version">2.0</resheader>
    <resheader name="reader">System.Resources.ResXResourceReader, System.Windows.Forms, ...</resheader>
    <resheader name="writer">System.Resources.ResXResourceWriter, System.Windows.Forms, ...</resheader>
    <data name="Name1"><value>this is my long string</value><comment>this is a comment</comment></data>
    <data name="Color1" type="System.Drawing.Color, System.Drawing">Blue</data>
    <data name="Bitmap1" mimetype="application/x-microsoft.net.object.binary.base64">
        <value>[base64 mime encoded serialized .NET Framework object]</value>
    </data>
    <data name="Icon1" type="System.Drawing.Icon, System.Drawing" mimetype="application/x-microsoft.net.object.bytearray.base64">
        <value>[base64 mime encoded string representing a byte array form of the .NET Framework object]</value>
        <comment>This is a comment</comment>
    </data>
                
    There are any number of "resheader" rows that contain simple 
    name/value pairs.
    
    Each data row contains a name, and value. The row also contains a 
    type or mimetype. Type corresponds to a .NET class that support 
    text/value conversion through the TypeConverter architecture. 
    Classes that don't support this are serialized and stored with the 
    mimetype set.
    
    The mimetype is used for serialized objects, and tells the 
    ResXResourceReader how to depersist the object. This is currently not 
    extensible. For a given mimetype the value must be set accordingly:
    
    Note - application/x-microsoft.net.object.binary.base64 is the format 
    that the ResXResourceWriter will generate, however the reader can 
    read any of the formats listed below.
    
    mimetype: application/x-microsoft.net.object.binary.base64
    value   : The object must be serialized with 
            : System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
            : and then encoded with base64 encoding.
    
    mimetype: application/x-microsoft.net.object.soap.base64
    value   : The object must be serialized with 
            : System.Runtime.Serialization.Formatters.Soap.SoapFormatter
            : and then encoded with base64 encoding.

    mimetype: application/x-microsoft.net.object.bytearray.base64
    value   : The object must be serialized into a byte array 
            : using a System.ComponentModel.TypeConverter
            : and then encoded with base64 encoding.
    -->
      <xsd:schema id="root" xmlns="" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
        <xsd:import namespace="http://www.w3.org/XML/1998/namespace" />
        <xsd:element name="root" msdata:IsDataSet="true">
          <xsd:complexType>
            <xsd:choice maxOccurs="unbounded">
              <xsd:element name="metadata">
                <xsd:complexType>
                  <xsd:sequence>
                    <xsd:element name="value" type="xsd:string" minOccurs="0" />
                  </xsd:sequence>
                  <xsd:attribute name="name" use="required" type="xsd:string" />
                  <xsd:attribute name="type" type="xsd:string" />
                  <xsd:attribute name="mimetype" type="xsd:string" />
                  <xsd:attribute ref="xml:space" />
                </xsd:complexType>
              </xsd:element>
              <xsd:element name="assembly">
                <xsd:complexType>
                  <xsd:attribute name="alias" type="xsd:string" />
                  <xsd:attribute name="name" type="xsd:string" />
                </xsd:complexType>
              </xsd:element>
              <xsd:element name="data">
                <xsd:complexType>
                  <xsd:sequence>
                    <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
                    <xsd:element name="comment" type="xsd:string" minOccurs="0" msdata:Ordinal="2" />
                  </xsd:sequence>
                  <xsd:attribute name="name" type="xsd:string" use="required" msdata:Ordinal="1" />
                  <xsd:attribute name="type" type="xsd:string" msdata:Ordinal="3" />
                  <xsd:attribute name="mimetype" type="xsd:string" msdata:Ordinal="4" />
                  <xsd:attribute ref="xml:space" />
                </xsd:complexType>
              </xsd:element>
              <xsd:element name="resheader">
                <xsd:complexType>
                  <xsd:sequence>
                    <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
                  </xsd:sequence>
                  <xsd:attribute name="name" type="xsd:string" use="required" />
                </xsd:complexType>
              </xsd:element>
            </xsd:choice>
          </xsd:complexType>
        </xsd:element>
      </xsd:schema>
      <resheader name="resmimetype">
        <value>text/microsoft-resx</value>
      </resheader>
      <resheader name="version">
        <value>2.0</value>
      </resheader>
      <resheader name="reader">
        <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
      </resheader>
      <resheader name="writer">
        <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
      </resheader>

      <!-- Начало данных -->

      <xsl:apply-templates select="vcr_two_dim/row">
        <xsl:sort select="@key"/>
      </xsl:apply-templates>

      <!-- конец данных -->

    </root>

  </xsl:template>

  <xsl:template match="row">

    <xsl:apply-templates select="entry">
      <xsl:sort select="@key"/>
    </xsl:apply-templates>

  </xsl:template>


  <xsl:template match="entry">
    <xsl:apply-templates select="lang"/>
  </xsl:template>

  <xsl:template match="lang">
    <xsl:apply-templates select="user"></xsl:apply-templates>
    <xsl:apply-templates select="prof"></xsl:apply-templates>
  </xsl:template>


  <xsl:template match="user | prof">
    <xsl:if test="../@langid=$cur_lang">
      <data>
        <xsl:attribute name="name">
          <xsl:value-of select="concat('Conclusion_', name(.), '_', ../../../@key, '_', ../../@key)"/>
          <!--            <xsl:value-of select="concat('Conclusion_', @key)"/> -->
        </xsl:attribute>

        <xsl:attribute name="xml:space">preserve</xsl:attribute>

        <value>
          <xsl:for-each select="para">

            <xsl:if test="position()!=1">
              <!-- если это не первый абзац текста, вставляем знак перевода строки -->
              <xsl:text>&#xa;</xsl:text>
            </xsl:if>

            <xsl:value-of select="."/>
          </xsl:for-each>
        </value>

        <comment>
          !!! DO NOT EDIT THIS FILE !!!<xsl:text>&#xa;</xsl:text>It is regenerated automagically (from two_dim_conclusions.xml) each time before the project is built! Your changes will be overwritten!
        </comment>

      </data>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>