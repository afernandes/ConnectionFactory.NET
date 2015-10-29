Framework Connection Factory .NET
===================

<ul>
<li><a href="#arquivo-de-configurações">Arquivo de Configurações</a></li>
<li><a href="#connectionfactorycfconnection">Class ConnectionFactory.CfConnection</a><ul>
<li><a href="#exemplo-de-conexão-com-o-banco-de-dados">Exemplo de conexão com o banco de dados</a></li>
<li><a href="#transação-com-múltiplas-conexões-transactionscope">Transação com múltiplas conexões (TransactionScope)</a></li>
<li><a href="#exemplo-de-query-retornando-entidade-carregada">Exemplo de Query retornando entidade carregada</a></li>
</ul>
</li>
<li><a href="#connectionfactorycfcommand">Class ConnectionFactory.CfCommand</a><ul>
<li><a href="#executescalar">ExecuteScalar</a></li>
<li><a href="#queryforobject">QueryForObject</a></li>
<li><a href="#queryforlist">QueryForList</a></li>
<li><a href="#executereader">ExecuteReader</a></li>
<li><a href="#executenonquery">ExecuteNonQuery</a></li>
</ul>
</li>
</ul>


##Objetivo
Oferecer um conjunto de funções para comunicação com banco de dados auxiliando no trabalho dos desenvolvedores de sistemas.

## Premissas
implementar funções similares ao iBATIS DAO simplificando a camada de persistência com o banco de dados, mas **sem a necessidade de utilizar os XML para mapeamento das entidades**. 

##Características

Suporte a todos provedores de bancos de dados que implementam o ADO.NET
: Tudo que é necessário para para se conectar a um novo banco é adicioná-lo ao arquivo de configurações  ([exemplo do Web.config](#web.config))
[Bancos de dados suportados (SQl Server, Oracle, MySQL, etc)](https://msdn.microsoft.com/pt-br/library/dd363565.aspx)

Conexão simplificada
: Conecta ao banco passando apenas o nome da conexão gravado no arquivo de configurações. ([exemplo](#conectando))

Transação entre servidores / bancos de dados
: Transações entre múltiplas conexões independente se estão em bancos de dados ou servidores distintos ([exemplo](#transactionScope))

Retorno automático de entidades (DTO / VO) carregadas a partir das querys
: Executa as querys e carrega as entidades automaticamente evitando linhas repetitivas de código para carregar cada membro do objeto. ([exemplo](#AutoLoadEntities))

### <a id="web.config">Arquivo de Configurações

```xml
<connectionStrings>
  <add name="DEFAULT" 
	   connectionString="Data Source=INSTANCIA;
					Initial Catalog=dtb_X;
					User Id=******; 
					Password=*****;
					App=NomeSistema;
					MultipleActiveResultSets=True;"    
	   providerName="System.Data.SqlClient"/>
</connectionStrings>
```

([Exemplo de chamada da conexão](#conectando))

##<a id="CfConnection">ConnectionFactory.CfConnection

### <a id="conectando">Exemplo de conexão com o banco de dados

```csharp
using (var conn = new CfConnection("DEFAULT"))
{
	...
}
```
([Exemplo do arquivo de configurações para a conexão "DEFAULT"](#web.config))


### <a id="transactionScope">Transação com múltiplas conexões (TransactionScope)

A `TransactionScope`pode ser utilizada em qualquer camada, tanto camada **DAO** com na **BO**

```csharp
try
{
   // Requer o MSDTC ativo
   using (var scope = new TransactionScope())
   {
   
      //Conexão com banco A no servidor X
      using (var conn = new CfConnection("DEFAULT"))
      {
         ...
      }
      
      //Conexão com o banco B no servidor Y
      using (var conn = new CfConnection("PORTAL"))
      {
         ...
      }

      //Commit em todas as conexões dentro do escopo
      scope.Complete();
   }
   catch (TransactionAbortedException tae)
   {
	  //Rollback devido erro na transação
	  ...
   }
   catch (Exception ex)
   {
      //Rollback automatico em todas as conexões dentro do TransactionScope em caso de erros
      ...
   }
}
```


### <a id="query">Exemplo de Query retornando entidade carregada

Executa a consulta no banco de dados e carrega automaticamente as entidades (VO / DTO), de forma simples, rápida e evitando erros de conversão incorreta.

```csharp
Vo.User user;
const string sql = @"SELECT * FROM SIS_USER WHERE ID = @Id";

using (var conn = new CfConnection("DEFAULT"))
{
   using (var cmd = conn.CreateCfCommand())
   {
      //Listagem de parâmetros
      var param = new List<CfParameter>
      {
         new CfParameter("@Id", id, DbType.Int32)
      };

      //Executa query e retorna DTO carregada
      user = cmd.QueryForObject<Vo.User>
		      (CommandType.Text, sql, param);
   }
}
```   

##<a id="CfCommand">ConnectionFactory.CfCommand


### <a id="ExecuteScalar">ExecuteScalar

Exemplo de um **INSERT** retornando o **ID**.

```csharp
using (var conn = new CfConnection("DEFAULT"))
{
   using (var cmd = conn.CreateCfCommand())
   {
      var parameters =
         new List<CfParameter>
         {
            new CfParameter("@Name", entity.Name),
            new CfParameter("@TimeStamp", entity.TimeStamp),
            new CfParameter("@IsActive", entity.IsActive),
            new CfParameter("@DisplayName", entity.DisplayName)
         };

      int id = cmd.ExecuteScalar<int>(CommandType.Text,
         @"INSERT INTO SIS_PROFILE
	         (NAME,  UPDATE_TIME, IS_ACTIVE, DISPLAY_NAME)
          VALUES 
	         (@Name, @TimeStamp,  @IsActive, @DisplayName)
           
          SELECT CAST(SCOPE_IDENTITY() AS INT) AS ID"
         , parameters);
   }
}
```


### <a id="QueryForObject">QueryForObject

Executa a **Query** ou **Procedure** e retorna uma entidade carregada

```csharp
Vo.User user;
const string sql = @"SELECT * FROM SIS_USER WHERE ID = @Id";

using (var conn = new CfConnection("DEFAULT"))
{
   using (var cmd = conn.CreateCfCommand())
   {
      var param = new List<CfParameter>
      {
         new CfParameter("@Id", id, DbType.Int32)
      };

      user = cmd.QueryForObject<Vo.User>(CommandType.Text, sql, param);
   }
}
```

### <a id="QueryForList">QueryForList

Executa uma query ou procedure e retorna uma Lista de entidades.

```csharp
IList<Resource> returnValue = null;

string sql = @"SELECT NAME RESOURCE_NAME
               FROM SIS_RESOURCE
               WHERE ENTITY_NAME=@EntityName";

using (var conn = new CfConnection(
	Util.ConnectionNames.DEFAULT.ToString()))
{
    using (var cmd = conn.CreateCfCommand())
	{
       var param = new List<CfParameter>{
          new CfParameter("@EntityName",entityType)};

       returnValue = cmd.QueryForList<Resource>
	       (CommandType.Text,sql,param);

    }
}
```

### <a id="ExecuteReader">ExecuteReader

Executa uma query ou procedure e retorna um **DataReader**

```csharp
IList<string> returnValue = null;
using (var conn = new CfConnection("DEFAULT"))
{
	using (var cmd = conn.CreateCfCommand())
    {
		const string sql = "SELECT NAME FROM SIS_ENTITY";
		using (var reader = cmd.ExecuteReader(CommandType.Text, sql))
		{
			if (reader.HasRows)
			{
	            returnValue = new List<String>();
	            while (reader.Read())
	            {
	                returnValue.Add(reader[0].ToString());
	            }
	        }
	    }
    }
}

return returnValue;
```

### <a id="ExecuteNonQuery">ExecuteNonQuery

Executa comando SQL sem retorno. Apropriado para INSERT, UPDATE e DELETE

```csharp
var param =
    new List<CfParameter>
    {
       new CfParameter("@Id", entity.Id),
       new CfParameter("@TimeStamp", entity.TimeStamp),
       new CfParameter("@Name", entity.Name),
       new CfParameter("@IsActive", entity.IsActive),
       new CfParameter("@DisplayName", entity.DisplayName)
    }; 

 using (var conn = new CfConnection("DEFAULT"))
 {
    using (var cmd = conn.CreateCfCommand())
    {
	   //Executa comando SQL sem retorno de valor
       cmd.ExecuteNonQuery(CommandType.Text, 
          @"UPDATE SIS_PROFILE
            SET UPDATE_TIME = @TimeStamp, NAME         = @Name, 
                IS_ACTIVE   = @IsActive,  DISPLAY_NAME = @DisplayName
            WHERE ID = @Id", param);
    }
 }
```
