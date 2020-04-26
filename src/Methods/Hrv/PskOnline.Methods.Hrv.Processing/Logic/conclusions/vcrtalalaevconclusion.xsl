<?xml version="1.0" encoding="UTF-8"?>
<!-- Edited by XMLSpy® -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/TR/WD-xsl">

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
        <th>Ключ, цвет</th>
        <th>Заключение для специалиста</th>
        <th>Заключение для неспециалиста</th>
      </tr>
      <xsl:for-each select="vcrtalalaev/entry" order-by="@key">
      <tr>
	  
        <td>
			<xsl:attribute name="class"><xsl:value-of select="@state_color"/></xsl:attribute>
			
			<xsl:value-of select="@key"/>
		</td>
        <td><xsl:value-of select="prof"/></td>
        <td><xsl:value-of select="user"/></td>
      </tr>
      </xsl:for-each>
    </table>
  </body>
  </html>
</xsl:template>
</xsl:stylesheet>

