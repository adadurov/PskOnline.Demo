<?xml version="1.0" encoding="UTF-8"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:template match="/">
    <html>
      <style>
        .red {
        background-color: #FF3333;
        }
        .green {
        background-color: #33FF33;
        }
        .yellow {
        background-color: #FFFF44;
        }
      </style>

      <body>
        <h2>Заключения по матрице состояний ВСР 5x5</h2>

        <p>Ключ: первый разряд -- строка матрицы (сверху вниз), второй разряд -- столбец матрицы (слева направо)</p>

        <table border="1">
          <tr bgcolor="#9acd32">
            <th>Ключ, цвет ячейки</th>
            <th>Заключение для специалиста</th>
            <th>Заключение для неспециалиста</th>
          </tr>

          <xsl:for-each select="vcr_two_dim/row">
            <xsl:sort select="@key"/>

            <xsl:for-each select="entry">

              <tr valign="top">
                <td colspan="3">
                  <p>
                    <br />
                  </p>
                </td>
              </tr>

              <xsl:for-each select="lang">

                <tr valign="top">


                  <xsl:if test="position()=1">
                    <td>
                      <xsl:attribute name="class">
                        <xsl:value-of select="../@state_color"/>
                      </xsl:attribute>
                      <xsl:attribute name="rowspan">
                        <xsl:value-of select="count(../lang)"/>
                      </xsl:attribute>
                      <xsl:value-of select="concat(../../@key, ../@key)"/>
                    </td>
                  </xsl:if>

                  <td width="50%">
                    <xsl:if test="count(prof)=0">
                      <p>
                        <br />
                      </p>
                    </xsl:if>

                    <xsl:for-each select="prof">
                      <h3>
                        <xsl:value-of select="../@langid"/>
                      </h3>
                      <xsl:for-each select="para">
                        <p>
                          <xsl:value-of select="."/>
                        </p>
                      </xsl:for-each>
                    </xsl:for-each>
                  </td>

                  <td width="50%">
                    <xsl:if test="count(prof)=0">
                      <p></p>
                    </xsl:if>
                    <xsl:for-each select="user">
                      <h3>
                        <xsl:value-of select="../@langid"/>
                      </h3>
                      <xsl:for-each select="para">
                        <p>
                          <xsl:value-of select="."/>
                        </p>
                      </xsl:for-each>
                    </xsl:for-each>
                  </td>

                </tr>
              </xsl:for-each>
            </xsl:for-each>
          </xsl:for-each>
        </table>
      </body>
    </html>

  </xsl:template>
</xsl:stylesheet>

