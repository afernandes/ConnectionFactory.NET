Framework Connection Factory .NET
===================

[TOC]

##Objetivo
Oferecer um conjunto de funções para comunicação com banco de dados auxiliando no trabalho dos desenvolvedores de sistemas.

## Premissas
implementar funções similares ao iBATIS DAO que simplificando a camada de persistência com o banco de dados, mas **não utilizar os XML de mapeamento**. 

##Características

Suporte a todos provedores de bancos de dados que implementam o ADO.NET
: Tudo que é necessário para para se conectar a um novo banco é adicioná-lo ao arquivo de configurações  ([exemplo do Web.config](#web.config))
[Bancos de dados suportados (SQl Server, Oracle, MySQL, etc)](https://msdn.microsoft.com/pt-br/library/dd363565.aspx)

Conexão simplificada
: Conecta ao banco passando apenas o nome da conexão gravado no arquivo de configurações. ([exemplo](#conectando))

Transação entre servidores / bancos de dados
: Transações entre múltiplas conexões independente se estão em bancos de dados ou servidores diferentes ([exemplo](#transactionScope))

Retorno automático de entidades (DTO / VO) carregadas a partir das querys
: Executa as querys e carrega as entidades automaticamente evitando linhas repetitivas de codigo para carregar cada membro do objeto. ([exemplo](#AutoLoadEntities))

##Pré-requisito

###Coordenador de Transações Distribuídas (MSDTC)

>MSDTC - Microsoft Distributed Transaction Coordinator 

#### O que faz o Coordenador de Transações Distribuídas?
O serviço DTC (Coordenador de Transações Distribuídas) coordena as transações que atualizam dois ou mais recursos protegidos por transação, como bancos de dados, filas de mensagens, sistemas de arquivos, entre outros. Esses recursos protegidos por transação podem estar em um único computador ou distribuídos em vários computadores em rede.

#### Configurando o serviço do MSDTC

Ativar o MSDTC
: Certifique-se de que o MSDTC está habilitado em ambos os servidores de banco de dados.

	- Em caso de aplicações Web, também deve estar ativo no servidor IIS
	- Em caso de aplicações Desktop, tambem deve estar ativo nas maquinas cliente que rodam o aplicativo.
	- Em todos os casos também deve estar ativo no computador dos desenvolvedores para possibilitar o debug das `TransactionScope`

1. **Ativar o Serviço do MSDTC**
	1. Vá em Iniciar -> Executar -> digite: "**Services.msc**"
	2. Ative o serviço: Coordenador de Transações Distribuídas

2. **Configurar o MSDTC**
	1. Vá em Iniciar -> Painel de Controle -> Ferramentas Administrativas -> **Serviços de Componentes** 
		1. Ou Executar: **%windir%\system32\comexp.msc**
		2. Ou Executar: **dcomcnfg.exe**

3. **Navegue até...**
	1. **No Windows 7**
	Expanda Raiz do console **->** Serviços de componente **->** Computadores **->** Meu computador **->** Coordenador de Transações Distribuídas **->** Local DTC
		**Propriedades do Botão direito do mouse**

 2. **No Windows XP**
 Expanda Raiz do console **->** Serviços de componente **->** Computadores **->** Meu computador 
**Propriedades do Botão direito do mouse**

4. **Permitir acesso ao DTC de Rede**
Vá na aba segurança e marque as opções 
- [x] **Acesso DTC de Rede**
- [x] **Permitir Entrada**
- [x] **Permitir Saída**
- [x] **Nenhuma Autenticação Necessária**

	**Clique em Ok e Reinicie o computador**
	[![](https://suneethasdiary.files.wordpress.com/2011/05/msdtc-settings2.jpg?w=274&h=300)](https://suneethasdiary.files.wordpress.com/2011/05/msdtc-settings2.jpg)

#### Checar se MSDTC está ativo no banco de dados

```sql
USE master
EXEC xp_servicecontrol N'querystate',N'msdtc'
```

#### Exemplo C# de uso do MSDTC

##### **Exemplo sem utilizar a Connection Factory**
```cs
using System.Transactions;

using (TransactionScope txSc = new TransactionScope())
{
	//Conexão com o primeiro banco de dados
	using (SqlConnection cn = new SqlConnection(connStr1))
    {
		SqlCommand cmd = cn.CreateCommand();
		cmd.CommandText = "Insert into Demo(DemoValue)  Values ('XXX')";
		cn.Open();
		cmd.ExecuteNonQuery();
		cn.Close();
	}

	//Conexão com o segundo banco de dados
	using (SqlConnection cn = new SqlConnection(connStr))
    {
		SqlCommand cmd = cn.CreateCommand();
		cmd.CommandText = "Insert into Demo(DemoValue) Values ('YYY')";
		cn.Open();
		cmd.ExecuteNonQuery();
		cn.Close();
	}

	Console.WriteLine( "Transaction identifier:" +
		Transaction.Current.TransactionInformation.
		DistributedIdentifier);
    
    //Commit nas duas conexões dentro do escopo da transação
    txSc.Complete();
}
```

##### **Exemplo utilizando a Connection Factory**

Exemplo de uso da `TransactionScope` dentro da Camada **BO**

```cs
public Int32 InsertOrUpdate(Vo.User user)
{
   Logger.Debug("Begin Method");
   int result = -1;

   try
   {
      // limit transaction scope
      using (var scope = new TransactionScope())
      {
         // call the UserDAO to save User in DB
         var userDAO = new UserDAO();
         result = userDAO.SaveOrUpdate(user);

         // delete all dependecies before insert/update new ones
         userDAO.DeleteUserDependencies(result);

         // profile list relationship
         if (user.ProfileList != null && user.ProfileList.Count > 0)
         {
            foreach (var profileUser in user.ProfileList)
            {
               if (profileUser != null)
               {
                  userDAO.InsertUserProfile(result, profileUser.Id);
               }
            }
         }

         // role list
         if (user.RolesList != null && user.RolesList.Count > 0)
         {
            foreach (var role in user.RolesList)
            {
               if (role != null)
               {
                  userDAO.InsertRole(result, role.Id);
               }
            }
         }


         scope.Complete();
      }
   }
   catch (TransactionAbortedException tae)
   {
      Logger.Error(tae);
      throw new BusinessException("Error on persistence layer", tae);
   }
   catch (LockException lockex)
   {
      Logger.Error(lockex);
      throw new LockException("Lock Exception", lockex);
   }
   catch (PersistenceException pex)
   {
      Logger.Error(pex);
      throw new BusinessException("Error on persistence layer", pex);
   }
   catch (Exception ex)
   {
      Logger.Error(ex);
      throw new BusinessException("An unexpected error has occured while handling business layer info.", ex);
   }

   Logger.Debug("End Method");

   return result;
}
```


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

```cs
using (var conn = new CfConnection("DEFAULT"))
{
	...
}
```
([Exemplo do arquivo de configurações para a conexão "DEFAULT"](#web.config))


### <a id="transactionScope">Transação com múltiplas conexões (TransactionScope)

A `TransactionScope`pode ser utilizada em qualquer camada, tanto camada **DAO** com na **BO**

```cs
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

```cs
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

```cs
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

```cs
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

```cs
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

```cs
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

```cs
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
