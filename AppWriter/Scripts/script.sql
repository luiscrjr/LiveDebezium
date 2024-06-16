CREATE DATABASE DBSync;
GO
USE DBSync
GO
CREATE TABLE [dbo].[TabelaDados](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Descricao] [nvarchar](125) NOT NULL,
	[Nome] [nvarchar](100),
	[Data] [date] NOT NULL,
	[DataAtualizacao] [datetime] NULL,
	[Acao] [varchar](1) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TabelaDados] ADD  DEFAULT (getdate()) FOR [Data]
GO

ALTER TABLE [dbo].[TabelaDados] ADD  DEFAULT (getdate()) FOR [DataAtualizacao]
GO

ALTER TABLE [dbo].[TabelaDados] ADD  CONSTRAINT [DF_TabelaDados_Acao]  DEFAULT ('I') FOR [Acao]
GO

CREATE TABLE [dbo].[TabelaDestino](
	[Id] [int] NOT NULL,
	[Descricao] [nvarchar](125) NOT NULL,
	[Nome] [nvarchar](100),
	[Data] [date] NOT NULL,
	[DataAtualizacao] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[TabelaDestino2](
	[Id] [int] NOT NULL,
	[Descricao] [nvarchar](125) NOT NULL,
	[Nome] [nvarchar](100),
	[Data] [date] NOT NULL,
	[DataAtualizacao] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
