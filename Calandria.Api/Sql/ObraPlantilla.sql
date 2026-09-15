/****** Object:  Table [dbo].[ActivacionTareasRuta]    Script Date: 7/31/2026 11:54:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ActivacionTareasRuta](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Manzana] [nvarchar](10) NOT NULL,
	[Lote] [nvarchar](10) NOT NULL,
	[Prototipo] [nvarchar](50) NULL,
	[Ruta] [nvarchar](50) NOT NULL,
	[NodoID] [int] NOT NULL,
	[NombreTarea] [nvarchar](200) NULL,
	[Activa] [bit] NOT NULL,
	[FechaActualizacion] [datetime] NULL,
	[UsuarioModificacion] [nvarchar](100) NULL,
	[CuadrillaAsignada] [nvarchar](20) NULL,
	[DesatajoActivado] [bit] NULL,
	[Finalizado] [bit] NULL,
	[FechaFinalizacion] [datetime] NULL,
	[FechaActivacion] [datetime] NULL,
	[NominaDistribuida] [bit] NOT NULL,
	[FechaDistribucionNomina] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AssignmentEstados]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AssignmentEstados](
	[Manzana] [nvarchar](50) NOT NULL,
	[Lote] [nvarchar](50) NOT NULL,
	[WBS] [nvarchar](200) NOT NULL,
	[Estado] [nvarchar](20) NOT NULL,
	[Usuario] [nvarchar](100) NOT NULL,
	[Fecha] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_AssignmentEstados] PRIMARY KEY CLUSTERED 
(
	[Manzana] ASC,
	[Lote] ASC,
	[WBS] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AvanceEstimacionConcepto_BACKUP]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AvanceEstimacionConcepto_BACKUP](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Codigo] [nvarchar](10) NULL,
	[Concepto] [nvarchar](200) NULL,
	[AvancePorcentaje] [float] NULL,
	[FechaActualizacion] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AvanceManualConcepto]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AvanceManualConcepto](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Manzana] [nvarchar](10) NULL,
	[Lote] [nvarchar](10) NULL,
	[Prototipo] [nvarchar](50) NULL,
	[Codigo] [nvarchar](10) NULL,
	[Concepto] [nvarchar](200) NULL,
	[AvancePorcentaje] [float] NULL,
	[FechaActualizacion] [datetime] NULL,
	[MontoEjecutado] [decimal](18, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_AvanceManualConcepto_Manzana_Lote_Codigo] UNIQUE NONCLUSTERED 
(
	[Manzana] ASC,
	[Lote] ASC,
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AvanceManualObra]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AvanceManualObra](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Manzana] [nvarchar](10) NULL,
	[Lote] [nvarchar](10) NULL,
	[Prototipo] [nvarchar](50) NULL,
	[WBS] [nvarchar](50) NULL,
	[AvancePorcentaje] [float] NULL,
	[FechaActualizacion] [datetime] NULL,
	[Concepto] [nvarchar](200) NULL,
	[MontoEjecutado] [decimal](18, 2) NULL,
	[ImporteTotal] [float] NULL,
	[FechaFinalizacion] [datetime] NULL,
	[MetrosCuadrados] [float] NULL,
	[IdPresupuestoObra] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AvanceManualObra_Backup_20250122]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AvanceManualObra_Backup_20250122](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Manzana] [nvarchar](10) NULL,
	[Lote] [nvarchar](10) NULL,
	[Prototipo] [nvarchar](50) NULL,
	[WBS] [nvarchar](50) NULL,
	[AvancePorcentaje] [float] NULL,
	[FechaActualizacion] [datetime] NULL,
	[Concepto] [nvarchar](200) NULL,
	[MontoEjecutado] [decimal](18, 2) NULL,
	[ImporteTotal] [float] NULL,
	[FechaFinalizacion] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AvanceManualObra_Backup_20260128]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AvanceManualObra_Backup_20260128](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Manzana] [nvarchar](10) NULL,
	[Lote] [nvarchar](10) NULL,
	[Prototipo] [nvarchar](50) NULL,
	[WBS] [nvarchar](50) NULL,
	[AvancePorcentaje] [float] NULL,
	[FechaActualizacion] [datetime] NULL,
	[Concepto] [nvarchar](200) NULL,
	[MontoEjecutado] [decimal](18, 2) NULL,
	[ImporteTotal] [float] NULL,
	[FechaFinalizacion] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AvanceManualObra_BACKUP_20260129_012715]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AvanceManualObra_BACKUP_20260129_012715](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Manzana] [nvarchar](10) NULL,
	[Lote] [nvarchar](10) NULL,
	[Prototipo] [nvarchar](50) NULL,
	[WBS] [nvarchar](50) NULL,
	[AvancePorcentaje] [float] NULL,
	[FechaActualizacion] [datetime] NULL,
	[Concepto] [nvarchar](200) NULL,
	[MontoEjecutado] [decimal](18, 2) NULL,
	[ImporteTotal] [float] NULL,
	[FechaFinalizacion] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AvanceManualObra_Backup_20260211_034446]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AvanceManualObra_Backup_20260211_034446](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Manzana] [nvarchar](10) NULL,
	[Lote] [nvarchar](10) NULL,
	[Prototipo] [nvarchar](50) NULL,
	[WBS] [nvarchar](50) NULL,
	[AvancePorcentaje] [float] NULL,
	[FechaActualizacion] [datetime] NULL,
	[Concepto] [nvarchar](200) NULL,
	[MontoEjecutado] [decimal](18, 2) NULL,
	[ImporteTotal] [float] NULL,
	[FechaFinalizacion] [datetime] NULL,
	[MetrosCuadrados] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AvanceManualObra_Backup_20260211_034853]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AvanceManualObra_Backup_20260211_034853](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Manzana] [nvarchar](10) NULL,
	[Lote] [nvarchar](10) NULL,
	[Prototipo] [nvarchar](50) NULL,
	[WBS] [nvarchar](50) NULL,
	[AvancePorcentaje] [float] NULL,
	[FechaActualizacion] [datetime] NULL,
	[Concepto] [nvarchar](200) NULL,
	[MontoEjecutado] [decimal](18, 2) NULL,
	[ImporteTotal] [float] NULL,
	[FechaFinalizacion] [datetime] NULL,
	[MetrosCuadrados] [float] NULL,
	[IdPresupuestoObra] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AvanceManualObra_BackupFEB2026]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AvanceManualObra_BackupFEB2026](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Manzana] [nvarchar](10) NULL,
	[Lote] [nvarchar](10) NULL,
	[Prototipo] [nvarchar](50) NULL,
	[WBS] [nvarchar](50) NULL,
	[AvancePorcentaje] [float] NULL,
	[FechaActualizacion] [datetime] NULL,
	[Concepto] [nvarchar](200) NULL,
	[MontoEjecutado] [decimal](18, 2) NULL,
	[ImporteTotal] [float] NULL,
	[FechaFinalizacion] [datetime] NULL,
	[MetrosCuadrados] [float] NULL,
	[IdPresupuestoObra] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AvanceObraManual]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AvanceObraManual](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Manzana] [nvarchar](50) NOT NULL,
	[Lote] [nvarchar](50) NOT NULL,
	[Categoria] [nvarchar](100) NOT NULL,
	[PorcentajeAvance] [decimal](5, 2) NOT NULL,
	[Ejecutado] [decimal](18, 2) NOT NULL,
	[Observaciones] [nvarchar](max) NULL,
	[UsuarioCaptura] [nvarchar](100) NOT NULL,
	[FechaCaptura] [datetime] NULL,
	[FechaModificacion] [datetime] NULL,
	[Presupuestado] [decimal](18, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_AvanceObra] UNIQUE NONCLUSTERED 
(
	[Manzana] ASC,
	[Lote] ASC,
	[Categoria] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CatalogoInsumos]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CatalogoInsumos](
	[Clave] [varchar](50) NOT NULL,
	[Descripcion] [nvarchar](200) NOT NULL,
	[Unidad] [nvarchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Clave] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Challenges]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Challenges](
	[Id] [nvarchar](64) NOT NULL,
	[Title] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[TargetJobs] [int] NOT NULL,
	[BonusAmount] [float] NOT NULL,
	[IsActive] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ChatMessages]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChatMessages](
	[Id] [nvarchar](64) NOT NULL,
	[JobId] [nvarchar](64) NOT NULL,
	[SenderId] [nvarchar](64) NOT NULL,
	[SenderName] [nvarchar](200) NULL,
	[ReceiverId] [nvarchar](64) NULL,
	[Message] [nvarchar](max) NOT NULL,
	[IsRead] [bit] NOT NULL,
	[Timestamp] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[COMPRASCALANDRA]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[COMPRASCALANDRA](
	[Clave] [nvarchar](255) NULL,
	[Descripcion] [nvarchar](255) NULL,
	[Unidad] [nvarchar](255) NULL,
	[Cantidad] [float] NULL,
	[Costo] [money] NULL,
	[Importe] [money] NULL,
	[Porcentaje] [float] NULL,
	[FAMILIA] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[COMPRASINDIRECTAS]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[COMPRASINDIRECTAS](
	[Clave] [nvarchar](255) NULL,
	[Descripcion] [nvarchar](255) NULL,
	[Unidad] [nvarchar](255) NULL,
	[Cantidad] [float] NULL,
	[Costo] [money] NULL,
	[Importe] [money] NULL,
	[Porcentaje] [float] NULL,
	[FAMILIA] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[COMPRASTUNERA]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[COMPRASTUNERA](
	[Clave] [nvarchar](255) NULL,
	[F2] [nvarchar](255) NULL,
	[Descripcion] [nvarchar](255) NULL,
	[F4] [nvarchar](255) NULL,
	[F5] [nvarchar](255) NULL,
	[Unidad] [nvarchar](255) NULL,
	[Cantidad] [float] NULL,
	[Costo] [money] NULL,
	[Importe] [money] NULL,
	[Porcentaje] [float] NULL,
	[Familia] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DestajoEvidencias]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DestajoEvidencias](
	[Manzana] [nvarchar](50) NOT NULL,
	[Lote] [nvarchar](50) NOT NULL,
	[WBSPadre] [nvarchar](200) NOT NULL,
	[FotoBytes] [varbinary](max) NOT NULL,
	[Usuario] [nvarchar](100) NOT NULL,
	[Fecha] [datetime2](7) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DestajoFotos]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DestajoFotos](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Manzana] [nvarchar](10) NOT NULL,
	[Lote] [nvarchar](10) NOT NULL,
	[Ruta] [nvarchar](50) NOT NULL,
	[NodoID] [int] NOT NULL,
	[Imagen] [varbinary](max) NOT NULL,
	[Caption] [nvarchar](300) NULL,
	[NombreArchivo] [nvarchar](255) NULL,
	[Usuario] [nvarchar](100) NULL,
	[FechaCreacion] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DestajoInsumos]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DestajoInsumos](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Manzana] [nvarchar](50) NOT NULL,
	[Lote] [nvarchar](50) NOT NULL,
	[WBS] [nvarchar](100) NULL,
	[InsumosJson] [nvarchar](max) NULL,
	[Fecha] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DestajoManoObra]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DestajoManoObra](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Manzana] [nvarchar](50) NOT NULL,
	[Lote] [nvarchar](50) NOT NULL,
	[WBS] [nvarchar](100) NULL,
	[ManoJson] [nvarchar](max) NULL,
	[Fecha] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DestajosTerminados]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DestajosTerminados](
	[Manzana] [nvarchar](50) NOT NULL,
	[Lote] [nvarchar](50) NOT NULL,
	[WBSPadre] [nvarchar](200) NOT NULL,
	[Usuario] [nvarchar](100) NOT NULL,
	[Fecha] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_DestajosTerminados] PRIMARY KEY CLUSTERED 
(
	[Manzana] ASC,
	[Lote] ASC,
	[WBSPadre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DeviceTokens]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DeviceTokens](
	[Token] [nvarchar](255) NOT NULL,
	[UserId] [nvarchar](64) NOT NULL,
	[Platform] [nvarchar](20) NULL,
	[UpdatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Token] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EntradasAlmacen]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EntradasAlmacen](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FolioOC] [varchar](50) NULL,
	[Clave] [varchar](50) NULL,
	[Descripcion] [nvarchar](255) NULL,
	[Unidad] [varchar](20) NULL,
	[Cantidad] [float] NULL,
	[FechaEntrada] [datetime] NULL,
	[Manzana] [varchar](10) NULL,
	[Lote] [varchar](10) NULL,
	[Prototipo] [varchar](50) NULL,
	[Usuario] [nvarchar](100) NULL,
	[EsExcepcion] [bit] NULL,
	[Justificacion] [nvarchar](max) NULL,
	[PrecioUnitario] [decimal](18, 2) NULL,
	[Importe] [decimal](18, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Estimacion(Concepto)]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Estimacion(Concepto)](
	[Código] [float] NULL,
	[Concepto] [nvarchar](255) NULL,
	[COSTOCALANDRA] [money] NULL,
	[COSTOTUNERA] [float] NULL,
	[Codigo] [nvarchar](10) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EvidenciasFotograficas]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EvidenciasFotograficas](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Manzana] [nvarchar](50) NOT NULL,
	[Lote] [nvarchar](50) NOT NULL,
	[Prototipo] [nvarchar](50) NULL,
	[TituloFoto] [nvarchar](200) NOT NULL,
	[Foto] [varbinary](max) NOT NULL,
	[Extension] [nvarchar](10) NULL,
	[TamanoKB] [float] NULL,
	[FechaCaptura] [datetime] NULL,
	[UsuarioCaptura] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExplosionInsumos]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExplosionInsumos](
	[Clave] [nvarchar](255) NULL,
	[F2] [nvarchar](255) NULL,
	[F3] [nvarchar](255) NULL,
	[Descripcion] [nvarchar](255) NULL,
	[F5] [nvarchar](255) NULL,
	[F6] [nvarchar](255) NULL,
	[F7] [nvarchar](255) NULL,
	[Unidad] [nvarchar](255) NULL,
	[Cantidad] [float] NULL,
	[F10] [float] NULL,
	[F11] [float] NULL,
	[Familia] [nvarchar](255) NULL,
	[PROTOTIPO] [nvarchar](255) NULL,
	[F14] [nvarchar](255) NULL,
	[F15] [nvarchar](255) NULL,
	[F16] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FoliosEstimacion]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FoliosEstimacion](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Folio] [nvarchar](50) NOT NULL,
	[Manzana] [nvarchar](10) NOT NULL,
	[Lote] [nvarchar](10) NOT NULL,
	[Prototipo] [nvarchar](50) NULL,
	[FechaGeneracion] [datetime] NULL,
	[Proveedor] [nvarchar](200) NULL,
	[Descripcion] [nvarchar](500) NULL,
	[ImporteContrato] [float] NULL,
	[TotalRequisicion] [float] NULL,
	[Amortizacion] [float] NULL,
	[PorcentajeAmortizacion] [float] NULL,
	[TotalEstimacion] [float] NULL,
	[NumeroEstimacion] [int] NULL,
	[Usuario] [nvarchar](100) NULL,
	[Observaciones] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Folio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FoliosEstimacionDetalle]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FoliosEstimacionDetalle](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FolioId] [int] NOT NULL,
	[CodigoConcepto] [nvarchar](10) NULL,
	[NombreConcepto] [nvarchar](200) NULL,
	[WBS] [int] NULL,
	[NombrePartida] [nvarchar](500) NULL,
	[MontoPresupuestado] [float] NULL,
	[MontoEjecutado] [float] NULL,
	[AvancePorcentaje] [float] NULL,
	[IdPresupuestoObra] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FoliosEstimacionDetalle_Backup_20260213_213839]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FoliosEstimacionDetalle_Backup_20260213_213839](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FolioId] [int] NOT NULL,
	[CodigoConcepto] [nvarchar](10) NULL,
	[NombreConcepto] [nvarchar](200) NULL,
	[WBS] [int] NULL,
	[NombrePartida] [nvarchar](500) NULL,
	[MontoPresupuestado] [float] NULL,
	[MontoEjecutado] [float] NULL,
	[AvancePorcentaje] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FoliosOrdenCompra]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FoliosOrdenCompra](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Folio] [nvarchar](50) NOT NULL,
	[Manzana] [nvarchar](10) NULL,
	[Lote] [nvarchar](10) NULL,
	[FechaGeneracion] [datetime] NULL,
	[TipoOrden] [nvarchar](20) NOT NULL,
	[NombreProveedor] [nvarchar](200) NULL,
	[CodigoProveedor] [nvarchar](50) NULL,
	[TotalSinIVA] [decimal](18, 2) NULL,
	[IVA] [decimal](18, 2) NULL,
	[TotalConIVA] [decimal](18, 2) NULL,
	[NumeroOrden] [int] NULL,
	[Usuario] [nvarchar](100) NULL,
	[Observaciones] [nvarchar](max) NULL,
	[Estado] [nvarchar](20) NULL,
	[NombreOrden] [nvarchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Folio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FoliosOrdenCompra_Casas]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FoliosOrdenCompra_Casas](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FolioId] [int] NOT NULL,
	[Manzana] [nvarchar](10) NOT NULL,
	[Lote] [nvarchar](10) NOT NULL,
	[Prototipo] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FoliosOrdenCompraDetalle]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FoliosOrdenCompraDetalle](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FolioId] [int] NOT NULL,
	[Clave] [nvarchar](100) NOT NULL,
	[Descripcion] [nvarchar](500) NULL,
	[Unidad] [nvarchar](50) NULL,
	[Cantidad] [decimal](18, 3) NOT NULL,
	[PrecioUnitario] [decimal](18, 2) NULL,
	[ImporteTotal] [decimal](18, 2) NULL,
	[Familia] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FoliosSalidaAlmacen]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FoliosSalidaAlmacen](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Folio] [nvarchar](50) NOT NULL,
	[Manzana] [nvarchar](10) NOT NULL,
	[Lote] [nvarchar](10) NOT NULL,
	[Prototipo] [nvarchar](50) NULL,
	[Obra] [nvarchar](200) NULL,
	[Edificacion] [nvarchar](200) NULL,
	[Urbanizacion] [nvarchar](200) NULL,
	[FechaSolicitud] [datetime] NOT NULL,
	[DiaSolicitud] [int] NULL,
	[MesSolicitud] [int] NULL,
	[AnioSolicitud] [int] NULL,
	[TotalImporte] [decimal](18, 2) NULL,
	[NumeroSalida] [int] NULL,
	[Usuario] [nvarchar](100) NULL,
	[Solicitante] [nvarchar](200) NULL,
	[ResidenteObra] [nvarchar](200) NULL,
	[EncargadoAlmacen] [nvarchar](200) NULL,
	[Observaciones] [nvarchar](max) NULL,
	[Estado] [nvarchar](20) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Folio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_FolioSalida] UNIQUE NONCLUSTERED 
(
	[Folio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FoliosSalidaAlmacenDetalle]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FoliosSalidaAlmacenDetalle](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FolioId] [int] NOT NULL,
	[NumeroFila] [int] NULL,
	[Codigo] [nvarchar](100) NULL,
	[Descripcion] [nvarchar](500) NULL,
	[Unidad] [nvarchar](50) NULL,
	[Cantidad] [decimal](18, 3) NULL,
	[PrecioUnitario] [decimal](18, 2) NULL,
	[Importe] [decimal](18, 2) NULL,
	[Observaciones] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FotosConcepto]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FotosConcepto](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Manzana] [nvarchar](50) NOT NULL,
	[Lote] [nvarchar](50) NOT NULL,
	[Identificador] [nvarchar](100) NOT NULL,
	[EsConcepto] [bit] NOT NULL,
	[NombreNodo] [nvarchar](255) NULL,
	[Descripcion] [nvarchar](255) NULL,
	[Foto] [varbinary](max) NOT NULL,
	[Extension] [nvarchar](10) NULL,
	[TamanioKB] [float] NULL,
	[FechaCaptura] [datetime] NOT NULL,
	[UsuarioCaptura] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HistorialMovimientos]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HistorialMovimientos](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Fecha] [datetime] NOT NULL,
	[TipoMovimiento] [nvarchar](10) NULL,
	[Clave] [nvarchar](100) NULL,
	[Descripcion] [nvarchar](255) NULL,
	[Unidad] [nvarchar](50) NULL,
	[Cantidad] [decimal](18, 2) NULL,
	[Usuario] [nvarchar](100) NULL,
	[Manzana] [nvarchar](50) NULL,
	[Lote] [nvarchar](50) NULL,
	[Prototipo] [nvarchar](100) NULL,
	[Justificacion] [nvarchar](max) NULL,
	[PrecioUnitario] [decimal](18, 2) NULL,
	[Importe] [decimal](18, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InsumosCalandraEXP]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InsumosCalandraEXP](
	[Clave] [nvarchar](50) NOT NULL,
	[Descripción] [nvarchar](1000) NULL,
	[Unidad] [nvarchar](20) NULL,
	[Cantidad] [decimal](18, 6) NULL,
	[Costo] [decimal](18, 4) NULL,
	[Importe] [decimal](18, 4) NULL,
	[Porcentaje] [decimal](18, 12) NULL,
	[Tipo] [nvarchar](50) NULL,
	[Familia] [nvarchar](80) NULL,
 CONSTRAINT [PK_InsumosCalandraEXP] PRIMARY KEY CLUSTERED 
(
	[Clave] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InsumosPorDestajo]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InsumosPorDestajo](
	[Destajo] [nvarchar](255) NULL,
	[Insumo] [nvarchar](255) NULL,
	[Clave] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InsumosTuneraEXP]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InsumosTuneraEXP](
	[Clave] [nvarchar](50) NOT NULL,
	[Descripción] [nvarchar](1000) NULL,
	[Unidad] [nvarchar](20) NULL,
	[Cantidad] [decimal](18, 6) NULL,
	[Costo] [decimal](18, 4) NULL,
	[Importe] [decimal](18, 4) NULL,
	[Porcentaje] [decimal](18, 12) NULL,
	[Tipo] [nvarchar](50) NULL,
	[Familia] [nvarchar](80) NULL,
 CONSTRAINT [PK_InsumosTuneraEXP] PRIMARY KEY CLUSTERED 
(
	[Clave] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InventarioActual]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InventarioActual](
	[Clave] [nvarchar](50) NOT NULL,
	[Descripcion] [nvarchar](200) NULL,
	[Unidad] [nvarchar](20) NULL,
	[CantidadDisponible] [decimal](18, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Clave] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InventarioCasas]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InventarioCasas](
	[Manzana] [nvarchar](50) NOT NULL,
	[Lote] [nvarchar](50) NOT NULL,
	[Prototipo] [nvarchar](255) NULL,
	[F4] [nvarchar](255) NULL,
	[FotoPath] [nvarchar](260) NULL,
	[DestajosTerminadosWBS] [nvarchar](max) NULL,
 CONSTRAINT [PK_InventarioCasas] PRIMARY KEY CLUSTERED 
(
	[Manzana] ASC,
	[Lote] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InventarioCasas_Backup_BEFORE_FIX]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InventarioCasas_Backup_BEFORE_FIX](
	[Manzana] [float] NULL,
	[Lote] [float] NULL,
	[Prototipo] [nvarchar](255) NULL,
	[F4] [nvarchar](255) NULL,
	[FotoPath] [nvarchar](260) NULL,
	[DestajosTerminadosWBS] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LogErrores]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LogErrores](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Fecha] [datetime] NOT NULL,
	[Equipo] [nvarchar](100) NULL,
	[Usuario] [nvarchar](100) NULL,
	[Version] [nvarchar](20) NULL,
	[Origen] [nvarchar](100) NULL,
	[TipoExcepcion] [nvarchar](200) NULL,
	[Mensaje] [nvarchar](max) NULL,
	[StackTrace] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ManoObraCalandra]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ManoObraCalandra](
	[Clave] [nvarchar](255) NULL,
	[Descripción] [nvarchar](255) NULL,
	[Unidad] [nvarchar](255) NULL,
	[Cantidad] [float] NULL,
	[Costo] [money] NULL,
	[Importe] [money] NULL,
	[Porcentaje] [float] NULL,
	[F8] [nvarchar](255) NULL,
	[F9] [nvarchar](255) NULL,
	[F10] [nvarchar](255) NULL,
	[F11] [nvarchar](255) NULL,
	[F12] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ManoObraTunera]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ManoObraTunera](
	[Clave] [nvarchar](255) NULL,
	[Descripción] [nvarchar](255) NULL,
	[Unidad] [nvarchar](255) NULL,
	[Cantidad] [float] NULL,
	[Costo] [money] NULL,
	[Importe] [money] NULL,
	[Porcentaje] [float] NULL,
	[F8] [nvarchar](255) NULL,
	[F9] [nvarchar](255) NULL,
	[F10] [nvarchar](255) NULL,
	[F11] [nvarchar](255) NULL,
	[F12] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MiembrosCuadrilla]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MiembrosCuadrilla](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CodigoCuadrilla] [varchar](50) NOT NULL,
	[Nombre] [nvarchar](100) NOT NULL,
	[Rol] [nvarchar](20) NOT NULL,
	[EsJefe] [bit] NOT NULL,
	[Telefono] [nvarchar](20) NULL,
	[Foto] [varbinary](max) NULL,
	[IdTrabajador] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NominaTareasAsignada]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NominaTareasAsignada](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Manzana] [nvarchar](10) NULL,
	[Lote] [nvarchar](10) NULL,
	[Ruta] [nvarchar](50) NULL,
	[NodoID] [int] NULL,
	[NombreTarea] [nvarchar](300) NULL,
	[CodigoCuadrilla] [nvarchar](20) NULL,
	[IdTrabajador] [int] NULL,
	[NombreTrabajador] [nvarchar](200) NULL,
	[Rol] [nvarchar](50) NULL,
	[EsJefe] [bit] NULL,
	[Monto] [decimal](18, 2) NULL,
	[TotalAsignado] [decimal](18, 2) NULL,
	[FechaActualizacion] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrdenesCompra]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrdenesCompra](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FolioOC] [nvarchar](100) NULL,
	[Fecha] [datetime] NULL,
	[Manzana] [nvarchar](50) NULL,
	[Lote] [nvarchar](50) NULL,
	[Clave] [nvarchar](100) NULL,
	[Descripcion] [nvarchar](255) NULL,
	[Unidad] [nvarchar](50) NULL,
	[Cantidad] [decimal](18, 2) NULL,
	[Proveedor] [nvarchar](100) NULL,
	[Usuario] [nvarchar](100) NULL,
	[Detalles] [nvarchar](max) NULL,
	[Estado] [nvarchar](20) NULL,
	[TipoOrden] [varchar](20) NOT NULL,
	[ProveedorClave] [nvarchar](50) NULL,
	[NombreOrden] [nvarchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_FolioOC] UNIQUE NONCLUSTERED 
(
	[FolioOC] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrdenesCompra_Casas]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrdenesCompra_Casas](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrdenId] [int] NULL,
	[Manzana] [nvarchar](20) NULL,
	[Lote] [nvarchar](20) NULL,
	[FolioOC] [varchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrdenesCompraDetalle]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrdenesCompraDetalle](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FolioOC] [nvarchar](50) NULL,
	[Clave] [nvarchar](50) NULL,
	[Descripcion] [nvarchar](500) NULL,
	[Unidad] [nvarchar](10) NULL,
	[Cantidad] [decimal](18, 4) NULL,
	[Familia] [nvarchar](100) NULL,
	[Estado] [varchar](20) NOT NULL,
	[Justificacion] [nvarchar](500) NULL,
	[PrecioUnitario] [decimal](18, 2) NULL,
	[ImporteTotal] [decimal](18, 2) NULL,
	[Categoria] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PDFsDestajos]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PDFsDestajos](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Manzana] [nvarchar](10) NOT NULL,
	[Lote] [nvarchar](10) NOT NULL,
	[Prototipo] [nvarchar](50) NULL,
	[Ruta] [nvarchar](50) NULL,
	[NodoID] [int] NULL,
	[NombreDestajo] [nvarchar](200) NULL,
	[CuadrillaAsignada] [nvarchar](20) NULL,
	[NombreArchivo] [nvarchar](255) NOT NULL,
	[ContenidoPDF] [varbinary](max) NOT NULL,
	[TamanioBytes] [bigint] NOT NULL,
	[Usuario] [nvarchar](100) NULL,
	[FechaGeneracion] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PDFsEstimacion]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PDFsEstimacion](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FolioId] [int] NOT NULL,
	[Folio] [nvarchar](50) NOT NULL,
	[Manzana] [nvarchar](10) NOT NULL,
	[Lote] [nvarchar](10) NOT NULL,
	[NombreArchivo] [nvarchar](255) NULL,
	[ContenidoPDF] [varbinary](max) NOT NULL,
	[TamanioBytes] [bigint] NULL,
	[FechaAlmacenamiento] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PDFsOrdenCompra]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PDFsOrdenCompra](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FolioId] [int] NOT NULL,
	[Folio] [nvarchar](50) NOT NULL,
	[Manzana] [nvarchar](10) NULL,
	[Lote] [nvarchar](10) NULL,
	[TipoOrden] [nvarchar](20) NOT NULL,
	[NombreArchivo] [nvarchar](255) NULL,
	[ContenidoPDF] [varbinary](max) NOT NULL,
	[TamanioBytes] [bigint] NULL,
	[FechaAlmacenamiento] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PDFsSalidaAlmacen]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PDFsSalidaAlmacen](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FolioId] [int] NOT NULL,
	[Folio] [nvarchar](50) NOT NULL,
	[Manzana] [nvarchar](10) NOT NULL,
	[Lote] [nvarchar](10) NOT NULL,
	[NombreArchivo] [nvarchar](255) NOT NULL,
	[ContenidoPDF] [varbinary](max) NOT NULL,
	[TamanioBytes] [bigint] NOT NULL,
	[FechaAlmacenamiento] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PendingWorkerRegistrations]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PendingWorkerRegistrations](
	[Id] [nvarchar](64) NOT NULL,
	[Name] [nvarchar](200) NULL,
	[Phone] [nvarchar](40) NULL,
	[Email] [nvarchar](200) NULL,
	[Username] [nvarchar](100) NOT NULL,
	[PasswordHash] [nvarchar](128) NULL,
	[Skills] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PhaseNegotiations]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PhaseNegotiations](
	[Id] [nvarchar](64) NOT NULL,
	[ProjectId] [nvarchar](64) NOT NULL,
	[PhaseId] [nvarchar](64) NOT NULL,
	[Status] [nvarchar](40) NOT NULL,
	[DataJson] [nvarchar](max) NOT NULL,
	[UpdatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PresupuestoObra]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PresupuestoObra](
	[Padre] [nvarchar](255) NULL,
	[Etapa] [nvarchar](255) NULL,
	[Partida] [nvarchar](255) NULL,
	[CostoCalandra] [float] NULL,
	[CostoTunera] [float] NULL,
	[Codigo] [nvarchar](10) NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[WBS_Correcto] [int] NULL,
	[EsDinamica] [bit] NOT NULL,
	[ValorM2Tunera] [float] NOT NULL,
	[ValorM2Calandra] [float] NOT NULL,
	[MetrosCuadrados] [float] NOT NULL,
	[LimiteM2] [float] NULL,
	[PrototiposAplicables] [nvarchar](255) NULL,
	[LimiteM2Tunera] [float] NULL,
	[LimiteM2Calandra] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PresupuestoObra_ANTES_REVERSION]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PresupuestoObra_ANTES_REVERSION](
	[Padre] [nvarchar](255) NULL,
	[Etapa] [nvarchar](255) NULL,
	[Partida] [nvarchar](255) NULL,
	[CostoCalandra] [float] NULL,
	[CostoTunera] [float] NULL,
	[Codigo] [nvarchar](10) NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PresupuestoObra_Backup_20260211_034446]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PresupuestoObra_Backup_20260211_034446](
	[Padre] [nvarchar](255) NULL,
	[Etapa] [nvarchar](255) NULL,
	[Partida] [nvarchar](255) NULL,
	[CostoCalandra] [float] NULL,
	[CostoTunera] [float] NULL,
	[Codigo] [nvarchar](10) NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[WBS_Correcto] [int] NULL,
	[EsDinamica] [bit] NOT NULL,
	[ValorM2Tunera] [float] NOT NULL,
	[ValorM2Calandra] [float] NOT NULL,
	[MetrosCuadrados] [float] NOT NULL,
	[LimiteM2] [float] NULL,
	[PrototiposAplicables] [nvarchar](255) NULL,
	[LimiteM2Tunera] [float] NULL,
	[LimiteM2Calandra] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PresupuestoObra_Backup_WBS]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PresupuestoObra_Backup_WBS](
	[Padre] [nvarchar](255) NULL,
	[Etapa] [nvarchar](255) NULL,
	[Partida] [nvarchar](255) NULL,
	[CostoCalandra] [float] NULL,
	[CostoTunera] [float] NULL,
	[Codigo] [nvarchar](10) NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[WBS_Correcto] [int] NULL,
	[EsDinamica] [bit] NOT NULL,
	[ValorM2Tunera] [float] NOT NULL,
	[ValorM2Calandra] [float] NOT NULL,
	[MetrosCuadrados] [float] NOT NULL,
	[LimiteM2] [float] NULL,
	[PrototiposAplicables] [nvarchar](255) NULL,
	[LimiteM2Tunera] [float] NULL,
	[LimiteM2Calandra] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PresupuestoObra_RESPALDO_EMERGENCIA]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PresupuestoObra_RESPALDO_EMERGENCIA](
	[Padre] [nvarchar](255) NULL,
	[Etapa] [nvarchar](255) NULL,
	[Partida] [nvarchar](255) NULL,
	[CostoCalandra] [float] NULL,
	[CostoTunera] [float] NULL,
	[Codigo] [nvarchar](10) NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProjectMessages]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProjectMessages](
	[Id] [nvarchar](64) NOT NULL,
	[ProjectId] [nvarchar](64) NOT NULL,
	[SenderId] [nvarchar](64) NOT NULL,
	[SenderName] [nvarchar](200) NULL,
	[Text] [nvarchar](max) NULL,
	[Attachment] [nvarchar](max) NULL,
	[AttachmentName] [nvarchar](200) NULL,
	[Timestamp] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Projects]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Projects](
	[Id] [nvarchar](64) NOT NULL,
	[ClientId] [nvarchar](64) NOT NULL,
	[ClientName] [nvarchar](200) NULL,
	[Title] [nvarchar](300) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Address] [nvarchar](400) NULL,
	[Status] [nvarchar](40) NOT NULL,
	[BudgetEstimated] [float] NOT NULL,
	[PhasesJson] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Promotions]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Promotions](
	[Id] [nvarchar](64) NOT NULL,
	[Title] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Code] [nvarchar](60) NULL,
	[DiscountValue] [float] NOT NULL,
	[Type] [nvarchar](40) NOT NULL,
	[ServiceType] [nvarchar](80) NULL,
	[FirstTimeOnly] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Proveedores]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Proveedores](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Codigo] [varchar](20) NOT NULL,
	[Nombre] [varchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PROVEEDORESCALANDRIA]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PROVEEDORESCALANDRIA](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ClaveUnica] [nvarchar](50) NOT NULL,
	[Nombre] [nvarchar](200) NOT NULL,
	[RFC] [nvarchar](13) NOT NULL,
	[Direccion] [nvarchar](500) NULL,
	[Telefono] [nvarchar](20) NULL,
	[FechaCreacion] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[ClaveUnica] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Ratings]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Ratings](
	[Id] [nvarchar](64) NOT NULL,
	[ProblemId] [nvarchar](64) NOT NULL,
	[WorkerId] [nvarchar](64) NOT NULL,
	[Rating] [int] NOT NULL,
	[Comment] [nvarchar](max) NULL,
	[Badges] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecibosNomina]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecibosNomina](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdTrabajador] [int] NULL,
	[NombreTrabajador] [nvarchar](200) NOT NULL,
	[Rol] [nvarchar](80) NULL,
	[CodigoCuadrilla] [nvarchar](20) NULL,
	[Concepto] [nvarchar](max) NULL,
	[Monto] [decimal](18, 2) NOT NULL,
	[FechaRecibo] [date] NOT NULL,
	[PeriodoDesde] [date] NULL,
	[PeriodoHasta] [date] NULL,
	[TotalCuadrilla] [decimal](18, 2) NULL,
	[Pdf] [varbinary](max) NULL,
	[Usuario] [nvarchar](120) NULL,
	[FechaCreacion] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RutaCalandraDestajo]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RutaCalandraDestajo](
	[ID] [int] NOT NULL,
	[ParentID] [int] NULL,
	[Nombre] [nvarchar](255) NOT NULL,
	[Descripcion] [nvarchar](500) NULL,
	[Orden] [int] NOT NULL,
	[Nivel] [int] NOT NULL,
	[TipoTarea] [int] NOT NULL,
	[FechaCreacion] [datetime] NOT NULL,
	[FechaModificacion] [datetime] NULL,
	[UsuarioCreacion] [nvarchar](100) NULL,
 CONSTRAINT [PK_RutaCalandraDestajo] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RutaCalandraDestajo_Columnas]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RutaCalandraDestajo_Columnas](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[NodoID] [int] NOT NULL,
	[NombreColumna] [nvarchar](100) NOT NULL,
	[Valor] [nvarchar](max) NULL,
 CONSTRAINT [PK_RutaCalandraDestajo_Columnas] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RutaCalandraDestajo_ColumnasDefinicion]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RutaCalandraDestajo_ColumnasDefinicion](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](100) NOT NULL,
	[Titulo] [nvarchar](100) NOT NULL,
	[Ancho] [int] NOT NULL,
	[TipoDato] [nvarchar](200) NOT NULL,
	[EsEditable] [bit] NOT NULL,
	[Formato] [nvarchar](50) NULL,
	[EsCalculada] [bit] NOT NULL,
	[TipoOperacion] [int] NOT NULL,
	[ColumnaOrigen1] [nvarchar](100) NULL,
	[ColumnaOrigen2] [nvarchar](100) NULL,
 CONSTRAINT [PK_RutaCalandraDestajo_ColumnasDefinicion] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_RutaCalandraDestajo_ColumnasDefinicion_Nombre] UNIQUE NONCLUSTERED 
(
	[Nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RutasCriticas]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RutasCriticas](
	[ID] [float] NULL,
	[WBS] [float] NULL,
	[Tarea] [nvarchar](255) NULL,
	[Cost] [money] NULL,
	[Work] [nvarchar](255) NULL,
	[Duration] [nvarchar](255) NULL,
	[Start] [nvarchar](255) NULL,
	[Finish] [nvarchar](255) NULL,
	[Regular Work] [nvarchar](255) NULL,
	[ClaveInsumo] [varchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RutaTuneraDestajo]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RutaTuneraDestajo](
	[ID] [int] NOT NULL,
	[ParentID] [int] NULL,
	[Nombre] [nvarchar](255) NOT NULL,
	[Descripcion] [nvarchar](500) NULL,
	[Orden] [int] NOT NULL,
	[Nivel] [int] NOT NULL,
	[FechaCreacion] [datetime] NOT NULL,
	[FechaModificacion] [datetime] NULL,
	[UsuarioCreacion] [nvarchar](100) NULL,
	[TipoTarea] [int] NOT NULL,
 CONSTRAINT [PK_TreeListData] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RutaTuneraDestajo_Columnas]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RutaTuneraDestajo_Columnas](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[NodoID] [int] NOT NULL,
	[NombreColumna] [nvarchar](100) NOT NULL,
	[Valor] [nvarchar](max) NULL,
 CONSTRAINT [PK_TreeListData_Columnas] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RutaTuneraDestajo_ColumnasDefinicion]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RutaTuneraDestajo_ColumnasDefinicion](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](100) NOT NULL,
	[Titulo] [nvarchar](100) NOT NULL,
	[Ancho] [int] NOT NULL,
	[TipoDato] [nvarchar](200) NOT NULL,
	[EsEditable] [bit] NOT NULL,
	[Formato] [nvarchar](50) NULL,
	[EsCalculada] [bit] NOT NULL,
	[TipoOperacion] [int] NOT NULL,
	[ColumnaOrigen1] [nvarchar](100) NULL,
	[ColumnaOrigen2] [nvarchar](100) NULL,
 CONSTRAINT [PK_RutaTuneraDestajo_ColumnasDefinicion] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_RutaTuneraDestajo_ColumnasDefinicion_Nombre] UNIQUE NONCLUSTERED 
(
	[Nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SalidasAlmacen]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SalidasAlmacen](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Fecha] [datetime] NOT NULL,
	[Clave] [nvarchar](100) NULL,
	[Descripcion] [nvarchar](255) NULL,
	[Unidad] [nvarchar](50) NULL,
	[Cantidad] [decimal](18, 2) NULL,
	[Usuario] [nvarchar](100) NULL,
	[Manzana] [nvarchar](50) NULL,
	[Lote] [nvarchar](50) NULL,
	[Prototipo] [nvarchar](100) NULL,
	[Justificacion] [nvarchar](max) NULL,
	[FechaSalida] [datetime] NOT NULL,
	[PrecioUnitario] [decimal](18, 2) NULL,
	[Importe] [decimal](18, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ServiceProblems]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ServiceProblems](
	[Id] [nvarchar](64) NOT NULL,
	[ClientId] [nvarchar](64) NOT NULL,
	[ClientName] [nvarchar](200) NULL,
	[ClientPhone] [nvarchar](40) NULL,
	[ServiceType] [nvarchar](80) NULL,
	[Description] [nvarchar](max) NULL,
	[PhotoPath] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NOT NULL,
	[Status] [nvarchar](40) NOT NULL,
	[Latitude] [float] NULL,
	[Longitude] [float] NULL,
	[Address] [nvarchar](400) NULL,
	[AssignedWorkerId] [nvarchar](64) NULL,
	[AssignedWorkerName] [nvarchar](200) NULL,
	[DiagnosisCost] [float] NULL,
	[RepairCost] [float] NULL,
	[TotalCost] [float] NULL,
	[CompletedAt] [datetime] NULL,
	[EvidencePhoto1] [nvarchar](max) NULL,
	[EvidencePhoto2] [nvarchar](max) NULL,
	[EvidencePhoto3] [nvarchar](max) NULL,
	[AppealDetails] [nvarchar](max) NULL,
	[WorkerLatitude] [float] NULL,
	[WorkerLongitude] [float] NULL,
	[WorkerBearing] [float] NULL,
	[ClientLatitude] [float] NULL,
	[ClientLongitude] [float] NULL,
	[AppliedPromotionId] [nvarchar](64) NULL,
	[PromotionDiscount] [float] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TRABAJADORES]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TRABAJADORES](
	[IdTrabajador] [int] IDENTITY(1,1) NOT NULL,
	[ClaveTrabajador] [nvarchar](20) NOT NULL,
	[Nombre] [nvarchar](200) NOT NULL,
	[Nacionalidad] [nvarchar](50) NOT NULL,
	[FechaNacimiento] [date] NULL,
	[CURP] [nvarchar](18) NULL,
	[RFC] [nvarchar](13) NULL,
	[INE] [nvarchar](20) NULL,
	[NSS] [nvarchar](11) NULL,
	[PDF_CURP] [varbinary](max) NULL,
	[PDF_RFC] [varbinary](max) NULL,
	[PDF_INE] [varbinary](max) NULL,
	[PDF_NSS] [varbinary](max) NULL,
	[FechaCreacion] [datetime] NOT NULL,
	[Foto] [varbinary](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[IdTrabajador] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[ClaveTrabajador] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [nvarchar](64) NOT NULL,
	[Name] [nvarchar](200) NULL,
	[Phone] [nvarchar](40) NULL,
	[Email] [nvarchar](200) NULL,
	[UserType] [nvarchar](20) NOT NULL,
	[ProfileImagePath] [nvarchar](max) NULL,
	[Rating] [float] NOT NULL,
	[CompletedJobs] [int] NOT NULL,
	[PasswordHash] [nvarchar](128) NULL,
	[CreatedAt] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsApproved] [bit] NOT NULL,
	[Username] [nvarchar](100) NOT NULL,
	[Badges] [nvarchar](max) NULL,
	[SavedAddresses] [nvarchar](max) NULL,
	[Salt] [nvarchar](64) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WorkerTools]    Script Date: 7/31/2026 11:54:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkerTools](
	[Id] [nvarchar](64) NOT NULL,
	[WorkerId] [nvarchar](64) NOT NULL,
	[JobId] [nvarchar](64) NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Cost] [float] NOT NULL,
	[ReceiptPhoto] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Index [IX_ActivacionTareasRuta_Activa]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_ActivacionTareasRuta_Activa] ON [dbo].[ActivacionTareasRuta]
(
	[Activa] ASC
)
INCLUDE([Manzana],[Lote],[NodoID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ActivacionTareasRuta_FechaActualizacion]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_ActivacionTareasRuta_FechaActualizacion] ON [dbo].[ActivacionTareasRuta]
(
	[FechaActualizacion] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ActivacionTareasRuta_ManzanaLote]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_ActivacionTareasRuta_ManzanaLote] ON [dbo].[ActivacionTareasRuta]
(
	[Manzana] ASC,
	[Lote] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ActivacionTareasRuta_Ruta]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_ActivacionTareasRuta_Ruta] ON [dbo].[ActivacionTareasRuta]
(
	[Ruta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_AvanceManualObra_IdPresupuestoObra]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_AvanceManualObra_IdPresupuestoObra] ON [dbo].[AvanceManualObra]
(
	[IdPresupuestoObra] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AvanceObra_Casa]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_AvanceObra_Casa] ON [dbo].[AvanceObraManual]
(
	[Manzana] ASC,
	[Lote] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AvanceObra_Categoria]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_AvanceObra_Categoria] ON [dbo].[AvanceObraManual]
(
	[Categoria] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AvanceObra_ManzanaLote]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_AvanceObra_ManzanaLote] ON [dbo].[AvanceObraManual]
(
	[Manzana] ASC,
	[Lote] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_DestajoFotos]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_DestajoFotos] ON [dbo].[DestajoFotos]
(
	[Manzana] ASC,
	[Lote] ASC,
	[Ruta] ASC,
	[NodoID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_DestajoInsumos_Key]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_DestajoInsumos_Key] ON [dbo].[DestajoInsumos]
(
	[Manzana] ASC,
	[Lote] ASC,
	[WBS] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_DestajoMano_Key]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_DestajoMano_Key] ON [dbo].[DestajoManoObra]
(
	[Manzana] ASC,
	[Lote] ASC,
	[WBS] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Evidencias_ManzanaLote]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_Evidencias_ManzanaLote] ON [dbo].[EvidenciasFotograficas]
(
	[Manzana] ASC,
	[Lote] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_FoliosEstimacion_ManzanaLote]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_FoliosEstimacion_ManzanaLote] ON [dbo].[FoliosEstimacion]
(
	[Manzana] ASC,
	[Lote] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_FoliosEstimacionDetalle_IdPresupuestoObra]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_FoliosEstimacionDetalle_IdPresupuestoObra] ON [dbo].[FoliosEstimacionDetalle]
(
	[IdPresupuestoObra] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_FoliosOrdenCompra_Fecha]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_FoliosOrdenCompra_Fecha] ON [dbo].[FoliosOrdenCompra]
(
	[FechaGeneracion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_FoliosOrdenCompra_ManzanaLote]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_FoliosOrdenCompra_ManzanaLote] ON [dbo].[FoliosOrdenCompra]
(
	[Manzana] ASC,
	[Lote] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_FoliosOrdenCompra_TipoOrden]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_FoliosOrdenCompra_TipoOrden] ON [dbo].[FoliosOrdenCompra]
(
	[TipoOrden] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_FoliosOrdenCompra_Casas_Casa]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_FoliosOrdenCompra_Casas_Casa] ON [dbo].[FoliosOrdenCompra_Casas]
(
	[Manzana] ASC,
	[Lote] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_FoliosOrdenCompra_Casas_FolioId]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_FoliosOrdenCompra_Casas_FolioId] ON [dbo].[FoliosOrdenCompra_Casas]
(
	[FolioId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_FoliosOrdenCompraDetalle_Clave]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_FoliosOrdenCompraDetalle_Clave] ON [dbo].[FoliosOrdenCompraDetalle]
(
	[Clave] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_FoliosOrdenCompraDetalle_FolioId]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_FoliosOrdenCompraDetalle_FolioId] ON [dbo].[FoliosOrdenCompraDetalle]
(
	[FolioId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_FotosConcepto_Nodo]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_FotosConcepto_Nodo] ON [dbo].[FotosConcepto]
(
	[Manzana] ASC,
	[Lote] ASC,
	[Identificador] ASC,
	[EsConcepto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_LogErrores_Fecha]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_LogErrores_Fecha] ON [dbo].[LogErrores]
(
	[Fecha] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PDFsDestajos_Fecha]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_PDFsDestajos_Fecha] ON [dbo].[PDFsDestajos]
(
	[FechaGeneracion] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_PDFsDestajos_ManzanaLote]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_PDFsDestajos_ManzanaLote] ON [dbo].[PDFsDestajos]
(
	[Manzana] ASC,
	[Lote] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_PDFsDestajos_Ruta]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_PDFsDestajos_Ruta] ON [dbo].[PDFsDestajos]
(
	[Ruta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_PDFsEstimacion_ManzanaLote]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_PDFsEstimacion_ManzanaLote] ON [dbo].[PDFsEstimacion]
(
	[Manzana] ASC,
	[Lote] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_PDFsOrdenCompra_Folio]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_PDFsOrdenCompra_Folio] ON [dbo].[PDFsOrdenCompra]
(
	[Folio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PDFsOrdenCompra_FolioId]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_PDFsOrdenCompra_FolioId] ON [dbo].[PDFsOrdenCompra]
(
	[FolioId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_PDFsOrdenCompra_ManzanaLote]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_PDFsOrdenCompra_ManzanaLote] ON [dbo].[PDFsOrdenCompra]
(
	[Manzana] ASC,
	[Lote] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_PDFsOrdenCompra_TipoOrden]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_PDFsOrdenCompra_TipoOrden] ON [dbo].[PDFsOrdenCompra]
(
	[TipoOrden] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_PDFsSalida_Folio]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_PDFsSalida_Folio] ON [dbo].[PDFsSalidaAlmacen]
(
	[Folio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_PDFsSalida_ManzanaLote]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_PDFsSalida_ManzanaLote] ON [dbo].[PDFsSalidaAlmacen]
(
	[Manzana] ASC,
	[Lote] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RutaCalandraDestajo_Nivel_Orden]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_RutaCalandraDestajo_Nivel_Orden] ON [dbo].[RutaCalandraDestajo]
(
	[Nivel] ASC,
	[Orden] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RutaCalandraDestajo_ParentID]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_RutaCalandraDestajo_ParentID] ON [dbo].[RutaCalandraDestajo]
(
	[ParentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RutaCalandraDestajo_TipoTarea]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_RutaCalandraDestajo_TipoTarea] ON [dbo].[RutaCalandraDestajo]
(
	[TipoTarea] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_RutaCalandraDestajo_Columnas_NodoID]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_RutaCalandraDestajo_Columnas_NodoID] ON [dbo].[RutaCalandraDestajo_Columnas]
(
	[NodoID] ASC,
	[NombreColumna] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RutaTuneraDestajo_Nivel_Orden]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_RutaTuneraDestajo_Nivel_Orden] ON [dbo].[RutaTuneraDestajo]
(
	[Nivel] ASC,
	[Orden] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RutaTuneraDestajo_ParentID]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_RutaTuneraDestajo_ParentID] ON [dbo].[RutaTuneraDestajo]
(
	[ParentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RutaTuneraDestajo_TipoTarea]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_RutaTuneraDestajo_TipoTarea] ON [dbo].[RutaTuneraDestajo]
(
	[TipoTarea] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_RutaTuneraDestajo_Columnas_NodoID]    Script Date: 7/31/2026 11:54:49 PM ******/
CREATE NONCLUSTERED INDEX [IX_RutaTuneraDestajo_Columnas_NodoID] ON [dbo].[RutaTuneraDestajo_Columnas]
(
	[NodoID] ASC,
	[NombreColumna] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ActivacionTareasRuta] ADD  DEFAULT ((1)) FOR [Activa]
GO
ALTER TABLE [dbo].[ActivacionTareasRuta] ADD  DEFAULT (getdate()) FOR [FechaActualizacion]
GO
ALTER TABLE [dbo].[ActivacionTareasRuta] ADD  DEFAULT ((0)) FOR [DesatajoActivado]
GO
ALTER TABLE [dbo].[ActivacionTareasRuta] ADD  DEFAULT ((0)) FOR [Finalizado]
GO
ALTER TABLE [dbo].[ActivacionTareasRuta] ADD  DEFAULT ((0)) FOR [NominaDistribuida]
GO
ALTER TABLE [dbo].[AssignmentEstados] ADD  DEFAULT (sysutcdatetime()) FOR [Fecha]
GO
ALTER TABLE [dbo].[AvanceEstimacionConcepto_BACKUP] ADD  DEFAULT (getdate()) FOR [FechaActualizacion]
GO
ALTER TABLE [dbo].[AvanceManualConcepto] ADD  DEFAULT (getdate()) FOR [FechaActualizacion]
GO
ALTER TABLE [dbo].[AvanceManualObra] ADD  DEFAULT ((0.0)) FOR [MetrosCuadrados]
GO
ALTER TABLE [dbo].[AvanceObraManual] ADD  DEFAULT (getdate()) FOR [FechaCaptura]
GO
ALTER TABLE [dbo].[AvanceObraManual] ADD  DEFAULT (getdate()) FOR [FechaModificacion]
GO
ALTER TABLE [dbo].[AvanceObraManual] ADD  DEFAULT ((0)) FOR [Presupuestado]
GO
ALTER TABLE [dbo].[Challenges] ADD  DEFAULT ((0)) FOR [TargetJobs]
GO
ALTER TABLE [dbo].[Challenges] ADD  DEFAULT ((0)) FOR [BonusAmount]
GO
ALTER TABLE [dbo].[Challenges] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[ChatMessages] ADD  DEFAULT ((0)) FOR [IsRead]
GO
ALTER TABLE [dbo].[ChatMessages] ADD  DEFAULT (getdate()) FOR [Timestamp]
GO
ALTER TABLE [dbo].[DestajoEvidencias] ADD  DEFAULT (sysutcdatetime()) FOR [Fecha]
GO
ALTER TABLE [dbo].[DestajoFotos] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[DestajoInsumos] ADD  DEFAULT (getdate()) FOR [Fecha]
GO
ALTER TABLE [dbo].[DestajoManoObra] ADD  DEFAULT (getdate()) FOR [Fecha]
GO
ALTER TABLE [dbo].[DestajosTerminados] ADD  DEFAULT (sysutcdatetime()) FOR [Fecha]
GO
ALTER TABLE [dbo].[DeviceTokens] ADD  DEFAULT (getdate()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[EntradasAlmacen] ADD  DEFAULT (getdate()) FOR [FechaEntrada]
GO
ALTER TABLE [dbo].[EvidenciasFotograficas] ADD  DEFAULT (getdate()) FOR [FechaCaptura]
GO
ALTER TABLE [dbo].[FoliosEstimacion] ADD  DEFAULT (getdate()) FOR [FechaGeneracion]
GO
ALTER TABLE [dbo].[FoliosOrdenCompra] ADD  DEFAULT (getdate()) FOR [FechaGeneracion]
GO
ALTER TABLE [dbo].[FoliosOrdenCompra] ADD  DEFAULT ((0)) FOR [TotalSinIVA]
GO
ALTER TABLE [dbo].[FoliosOrdenCompra] ADD  DEFAULT ((0)) FOR [IVA]
GO
ALTER TABLE [dbo].[FoliosOrdenCompra] ADD  DEFAULT ((0)) FOR [TotalConIVA]
GO
ALTER TABLE [dbo].[FoliosOrdenCompra] ADD  DEFAULT ('PENDIENTE') FOR [Estado]
GO
ALTER TABLE [dbo].[FoliosOrdenCompraDetalle] ADD  DEFAULT ((0)) FOR [PrecioUnitario]
GO
ALTER TABLE [dbo].[FoliosOrdenCompraDetalle] ADD  DEFAULT ((0)) FOR [ImporteTotal]
GO
ALTER TABLE [dbo].[FoliosSalidaAlmacen] ADD  DEFAULT (getdate()) FOR [FechaSolicitud]
GO
ALTER TABLE [dbo].[FoliosSalidaAlmacen] ADD  DEFAULT ((0)) FOR [TotalImporte]
GO
ALTER TABLE [dbo].[FoliosSalidaAlmacen] ADD  DEFAULT ('PENDIENTE') FOR [Estado]
GO
ALTER TABLE [dbo].[FotosConcepto] ADD  DEFAULT ((1)) FOR [EsConcepto]
GO
ALTER TABLE [dbo].[FotosConcepto] ADD  DEFAULT (getdate()) FOR [FechaCaptura]
GO
ALTER TABLE [dbo].[HistorialMovimientos] ADD  DEFAULT (getdate()) FOR [Fecha]
GO
ALTER TABLE [dbo].[MiembrosCuadrilla] ADD  DEFAULT ((0)) FOR [EsJefe]
GO
ALTER TABLE [dbo].[NominaTareasAsignada] ADD  DEFAULT (getdate()) FOR [FechaActualizacion]
GO
ALTER TABLE [dbo].[OrdenesCompra] ADD  DEFAULT ('PENDIENTE') FOR [Estado]
GO
ALTER TABLE [dbo].[OrdenesCompra] ADD  DEFAULT ('INDIVIDUAL') FOR [TipoOrden]
GO
ALTER TABLE [dbo].[OrdenesCompraDetalle] ADD  DEFAULT ('PENDIENTE') FOR [Estado]
GO
ALTER TABLE [dbo].[PDFsDestajos] ADD  DEFAULT (getdate()) FOR [FechaGeneracion]
GO
ALTER TABLE [dbo].[PDFsEstimacion] ADD  DEFAULT (getdate()) FOR [FechaAlmacenamiento]
GO
ALTER TABLE [dbo].[PDFsOrdenCompra] ADD  DEFAULT (getdate()) FOR [FechaAlmacenamiento]
GO
ALTER TABLE [dbo].[PDFsSalidaAlmacen] ADD  DEFAULT (getdate()) FOR [FechaAlmacenamiento]
GO
ALTER TABLE [dbo].[PendingWorkerRegistrations] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[PhaseNegotiations] ADD  DEFAULT ('Negociando') FOR [Status]
GO
ALTER TABLE [dbo].[PhaseNegotiations] ADD  DEFAULT (getdate()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[PresupuestoObra] ADD  DEFAULT ((0)) FOR [EsDinamica]
GO
ALTER TABLE [dbo].[PresupuestoObra] ADD  DEFAULT ((0)) FOR [ValorM2Tunera]
GO
ALTER TABLE [dbo].[PresupuestoObra] ADD  DEFAULT ((0)) FOR [ValorM2Calandra]
GO
ALTER TABLE [dbo].[PresupuestoObra] ADD  DEFAULT ((0)) FOR [MetrosCuadrados]
GO
ALTER TABLE [dbo].[PresupuestoObra] ADD  DEFAULT ((0)) FOR [LimiteM2]
GO
ALTER TABLE [dbo].[PresupuestoObra] ADD  DEFAULT ((0)) FOR [LimiteM2Tunera]
GO
ALTER TABLE [dbo].[PresupuestoObra] ADD  DEFAULT ((0)) FOR [LimiteM2Calandra]
GO
ALTER TABLE [dbo].[ProjectMessages] ADD  DEFAULT (getdate()) FOR [Timestamp]
GO
ALTER TABLE [dbo].[Projects] ADD  DEFAULT ('Planificacion') FOR [Status]
GO
ALTER TABLE [dbo].[Projects] ADD  DEFAULT ((0)) FOR [BudgetEstimated]
GO
ALTER TABLE [dbo].[Projects] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Promotions] ADD  DEFAULT ((0)) FOR [DiscountValue]
GO
ALTER TABLE [dbo].[Promotions] ADD  DEFAULT ('Percentage') FOR [Type]
GO
ALTER TABLE [dbo].[Promotions] ADD  DEFAULT ((0)) FOR [FirstTimeOnly]
GO
ALTER TABLE [dbo].[Promotions] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[PROVEEDORESCALANDRIA] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Ratings] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[RecibosNomina] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[RutaCalandraDestajo] ADD  DEFAULT ((0)) FOR [Orden]
GO
ALTER TABLE [dbo].[RutaCalandraDestajo] ADD  DEFAULT ((0)) FOR [Nivel]
GO
ALTER TABLE [dbo].[RutaCalandraDestajo] ADD  DEFAULT ((0)) FOR [TipoTarea]
GO
ALTER TABLE [dbo].[RutaCalandraDestajo] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[RutaCalandraDestajo] ADD  DEFAULT ('Sistema') FOR [UsuarioCreacion]
GO
ALTER TABLE [dbo].[RutaCalandraDestajo_ColumnasDefinicion] ADD  DEFAULT ((100)) FOR [Ancho]
GO
ALTER TABLE [dbo].[RutaCalandraDestajo_ColumnasDefinicion] ADD  DEFAULT ('System.String') FOR [TipoDato]
GO
ALTER TABLE [dbo].[RutaCalandraDestajo_ColumnasDefinicion] ADD  DEFAULT ((1)) FOR [EsEditable]
GO
ALTER TABLE [dbo].[RutaCalandraDestajo_ColumnasDefinicion] ADD  DEFAULT ((0)) FOR [EsCalculada]
GO
ALTER TABLE [dbo].[RutaCalandraDestajo_ColumnasDefinicion] ADD  DEFAULT ((0)) FOR [TipoOperacion]
GO
ALTER TABLE [dbo].[RutaTuneraDestajo] ADD  DEFAULT ((0)) FOR [Orden]
GO
ALTER TABLE [dbo].[RutaTuneraDestajo] ADD  DEFAULT ((0)) FOR [Nivel]
GO
ALTER TABLE [dbo].[RutaTuneraDestajo] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[RutaTuneraDestajo] ADD  DEFAULT ('Sistema') FOR [UsuarioCreacion]
GO
ALTER TABLE [dbo].[RutaTuneraDestajo] ADD  DEFAULT ((0)) FOR [TipoTarea]
GO
ALTER TABLE [dbo].[RutaTuneraDestajo_ColumnasDefinicion] ADD  DEFAULT ((100)) FOR [Ancho]
GO
ALTER TABLE [dbo].[RutaTuneraDestajo_ColumnasDefinicion] ADD  DEFAULT ('System.String') FOR [TipoDato]
GO
ALTER TABLE [dbo].[RutaTuneraDestajo_ColumnasDefinicion] ADD  DEFAULT ((1)) FOR [EsEditable]
GO
ALTER TABLE [dbo].[RutaTuneraDestajo_ColumnasDefinicion] ADD  DEFAULT ((0)) FOR [EsCalculada]
GO
ALTER TABLE [dbo].[RutaTuneraDestajo_ColumnasDefinicion] ADD  DEFAULT ((0)) FOR [TipoOperacion]
GO
ALTER TABLE [dbo].[SalidasAlmacen] ADD  DEFAULT (getdate()) FOR [Fecha]
GO
ALTER TABLE [dbo].[SalidasAlmacen] ADD  DEFAULT (getdate()) FOR [FechaSalida]
GO
ALTER TABLE [dbo].[ServiceProblems] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[ServiceProblems] ADD  DEFAULT ('Abierto') FOR [Status]
GO
ALTER TABLE [dbo].[ServiceProblems] ADD  DEFAULT ((0)) FOR [PromotionDiscount]
GO
ALTER TABLE [dbo].[TRABAJADORES] ADD  DEFAULT (getdate()) FOR [FechaCreacion]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((5.0)) FOR [Rating]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((0)) FOR [CompletedJobs]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((1)) FOR [IsApproved]
GO
ALTER TABLE [dbo].[WorkerTools] ADD  DEFAULT ((0)) FOR [Cost]
GO
ALTER TABLE [dbo].[WorkerTools] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[ActivacionTareasRuta]  WITH CHECK ADD  CONSTRAINT [FK_ActivacionTareasRuta_NodoID] FOREIGN KEY([NodoID])
REFERENCES [dbo].[RutaTuneraDestajo] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ActivacionTareasRuta] CHECK CONSTRAINT [FK_ActivacionTareasRuta_NodoID]
GO
ALTER TABLE [dbo].[EvidenciasFotograficas]  WITH CHECK ADD  CONSTRAINT [FK_Evidencias_Inventario] FOREIGN KEY([Manzana], [Lote])
REFERENCES [dbo].[InventarioCasas] ([Manzana], [Lote])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EvidenciasFotograficas] CHECK CONSTRAINT [FK_Evidencias_Inventario]
GO
ALTER TABLE [dbo].[FoliosEstimacionDetalle]  WITH CHECK ADD FOREIGN KEY([FolioId])
REFERENCES [dbo].[FoliosEstimacion] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FoliosOrdenCompra_Casas]  WITH CHECK ADD FOREIGN KEY([FolioId])
REFERENCES [dbo].[FoliosOrdenCompra] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FoliosOrdenCompraDetalle]  WITH CHECK ADD FOREIGN KEY([FolioId])
REFERENCES [dbo].[FoliosOrdenCompra] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FoliosSalidaAlmacenDetalle]  WITH CHECK ADD  CONSTRAINT [FK_FoliosSalidaDetalle_Folio] FOREIGN KEY([FolioId])
REFERENCES [dbo].[FoliosSalidaAlmacen] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FoliosSalidaAlmacenDetalle] CHECK CONSTRAINT [FK_FoliosSalidaDetalle_Folio]
GO
ALTER TABLE [dbo].[OrdenesCompra_Casas]  WITH CHECK ADD FOREIGN KEY([OrdenId])
REFERENCES [dbo].[OrdenesCompra] ([Id])
GO
ALTER TABLE [dbo].[PDFsEstimacion]  WITH CHECK ADD FOREIGN KEY([FolioId])
REFERENCES [dbo].[FoliosEstimacion] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PDFsOrdenCompra]  WITH CHECK ADD FOREIGN KEY([FolioId])
REFERENCES [dbo].[FoliosOrdenCompra] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PDFsSalidaAlmacen]  WITH CHECK ADD  CONSTRAINT [FK_PDFsSalida_Folio] FOREIGN KEY([FolioId])
REFERENCES [dbo].[FoliosSalidaAlmacen] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PDFsSalidaAlmacen] CHECK CONSTRAINT [FK_PDFsSalida_Folio]
GO
ALTER TABLE [dbo].[RutaCalandraDestajo_Columnas]  WITH CHECK ADD  CONSTRAINT [FK_RutaCalandraDestajo_Columnas_Nodo] FOREIGN KEY([NodoID])
REFERENCES [dbo].[RutaCalandraDestajo] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RutaCalandraDestajo_Columnas] CHECK CONSTRAINT [FK_RutaCalandraDestajo_Columnas_Nodo]
GO
ALTER TABLE [dbo].[RutaTuneraDestajo_Columnas]  WITH CHECK ADD  CONSTRAINT [FK_RutaTuneraDestajo_Columnas_Nodo] FOREIGN KEY([NodoID])
REFERENCES [dbo].[RutaTuneraDestajo] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RutaTuneraDestajo_Columnas] CHECK CONSTRAINT [FK_RutaTuneraDestajo_Columnas_Nodo]
GO
ALTER TABLE [dbo].[MiembrosCuadrilla]  WITH CHECK ADD CHECK  (([Rol]='OFICIAL' OR [Rol]='AYUDANTE' OR [Rol]='PEON'))
GO
/****** Object:  View [dbo].[vw_AvanceConceptosConsolidado] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW vw_AvanceConceptosConsolidado AS
SELECT
    amc.Manzana,
    amc.Lote,
    amc.Prototipo,
    amc.Codigo,
    amc.Concepto,
    amc.AvancePorcentaje,
    CASE
        WHEN amc.Manzana = 'GLOBAL' AND amc.Lote = 'GLOBAL' THEN 'Global'
        ELSE 'Específico'
    END as TipoAvance,
    amc.FechaActualizacion
FROM AvanceManualConcepto amc
GO
/****** Object:  View [dbo].[VW_FoliosEstimacionResumen] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW VW_FoliosEstimacionResumen AS
SELECT
    f.Id,
    f.Folio,
    f.Manzana,
    f.Lote,
    f.Prototipo,
    f.FechaGeneracion,
    f.Proveedor,
    f.TotalEstimacion,
    f.NumeroEstimacion,
    COUNT(d.Id) AS TotalPartidas,
    CASE WHEN p.Id IS NOT NULL THEN 1 ELSE 0 END AS TienePDF
FROM FoliosEstimacion f
LEFT JOIN FoliosEstimacionDetalle d ON f.Id = d.FolioId
LEFT JOIN PDFsEstimacion p ON f.Id = p.FolioId
GROUP BY f.Id, f.Folio, f.Manzana, f.Lote, f.Prototipo, f.FechaGeneracion,
         f.Proveedor, f.TotalEstimacion, f.NumeroEstimacion, p.Id
GO
/****** Object:  View [dbo].[VW_FoliosOrdenCompraResumen] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW VW_FoliosOrdenCompraResumen AS
SELECT
    f.Id,
    f.Folio,
    f.Manzana,
    f.Lote,
    f.FechaGeneracion,
    f.TipoOrden,
    f.NombreProveedor,
    f.CodigoProveedor,
    f.TotalConIVA,
    f.NumeroOrden,
    f.Estado,
    COUNT(DISTINCT d.Id) AS TotalInsumos,
    CASE WHEN p.Id IS NOT NULL THEN 1 ELSE 0 END AS TienePDF,
    STUFF((
        SELECT ', M' + c.Manzana + '-L' + c.Lote
        FROM FoliosOrdenCompra_Casas c
        WHERE c.FolioId = f.Id
        FOR XML PATH(''), TYPE
    ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS CasasIncluidas
FROM FoliosOrdenCompra f
LEFT JOIN FoliosOrdenCompraDetalle d ON f.Id = d.FolioId
LEFT JOIN PDFsOrdenCompra p ON f.Id = p.FolioId
GROUP BY f.Id, f.Folio, f.Manzana, f.Lote, f.FechaGeneracion, f.TipoOrden,
         f.NombreProveedor, f.CodigoProveedor, f.TotalConIVA, f.NumeroOrden, f.Estado, p.Id
GO
/****** Object:  View [dbo].[VW_FoliosSalidaResumen] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[VW_FoliosSalidaResumen]
AS
SELECT
    f.Id,
    f.Folio,
    f.Manzana,
    f.Lote,
    f.Prototipo,
    f.Obra,
    f.FechaSolicitud,
    f.TotalImporte,
    f.NumeroSalida,
    f.Usuario,
    f.Solicitante,
    f.Estado,
    (SELECT COUNT(*) FROM FoliosSalidaAlmacenDetalle WHERE FolioId = f.Id) AS TotalInsumos,
    (SELECT CASE WHEN EXISTS(SELECT 1 FROM PDFsSalidaAlmacen WHERE FolioId = f.Id)
        THEN 'SI' ELSE 'NO' END) AS TienePDF
FROM FoliosSalidaAlmacen f
GO
/****** Object:  View [dbo].[VW_OrdenesCompraPorCasa] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW VW_OrdenesCompraPorCasa AS
SELECT DISTINCT
    fc.Manzana,
    fc.Lote,
    f.Folio,
    f.TipoOrden,
    f.FechaGeneracion,
    f.NombreProveedor,
    f.TotalConIVA,
    f.Estado,
    (SELECT COUNT(*) FROM FoliosOrdenCompraDetalle WHERE FolioId = f.Id) AS TotalInsumos
FROM FoliosOrdenCompra_Casas fc
INNER JOIN FoliosOrdenCompra f ON fc.FolioId = f.Id

UNION

SELECT
    f.Manzana,
    f.Lote,
    f.Folio,
    f.TipoOrden,
    f.FechaGeneracion,
    f.NombreProveedor,
    f.TotalConIVA,
    f.Estado,
    (SELECT COUNT(*) FROM FoliosOrdenCompraDetalle WHERE FolioId = f.Id) AS TotalInsumos
FROM FoliosOrdenCompra f
WHERE f.Manzana IS NOT NULL AND f.Lote IS NOT NULL
GO
/****** Object:  View [dbo].[vw_TareasActivasPorCasa] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW dbo.vw_TareasActivasPorCasa
AS
SELECT
    atr.Manzana,
    atr.Lote,
    atr.Prototipo,
    atr.Ruta,
    atr.NodoID,
    atr.NombreTarea,
    atr.Activa,
    atr.FechaActualizacion,
    atr.UsuarioModificacion,
    CASE
        WHEN atr.Ruta LIKE '%Calandria%' THEN 'Calandria'
        WHEN atr.Ruta LIKE '%Tunera%' THEN 'Tunera'
        ELSE 'Desconocido'
    END AS TipoRuta
FROM ActivacionTareasRuta atr
WHERE atr.Activa = 1
GO
/****** Datos iniciales: columnas por defecto del Editor de Tareas (Duración/Unidad/Cantidad/Precio/Total),
        replicadas de CALANDRIA (obra original) para que toda obra nueva las tenga desde su creación. ******/
INSERT INTO [dbo].[RutaTuneraDestajo_ColumnasDefinicion]
    (Nombre, Titulo, Ancho, TipoDato, EsEditable, Formato, EsCalculada, TipoOperacion, ColumnaOrigen1, ColumnaOrigen2)
VALUES
    (N'Duración', N'Duración', 100, N'System.String', 1, NULL, 0, 0, NULL, NULL),
    (N'Unidad', N'Unidad', 100, N'System.String', 1, NULL, 0, 0, NULL, NULL),
    (N'Cantidad', N'Cantidad', 100, N'System.Decimal', 1, N'{0:N2}', 0, 0, NULL, NULL),
    (N'Precio', N'Precio Unitario', 120, N'System.Decimal', 1, N'{0:C2}', 0, 0, NULL, NULL),
    (N'Total', N'Total', 130, N'System.Decimal', 0, N'{0:C2}', 1, 1, N'Precio', N'Cantidad')
GO
INSERT INTO [dbo].[RutaCalandraDestajo_ColumnasDefinicion]
    (Nombre, Titulo, Ancho, TipoDato, EsEditable, Formato, EsCalculada, TipoOperacion, ColumnaOrigen1, ColumnaOrigen2)
VALUES
    (N'Duración', N'Duración', 100, N'System.String', 1, NULL, 0, 0, NULL, NULL),
    (N'Unidad', N'Unidad', 100, N'System.String', 1, NULL, 0, 0, NULL, NULL),
    (N'Cantidad', N'Cantidad', 100, N'System.Decimal', 1, N'{0:N2}', 0, 0, NULL, NULL),
    (N'Precio', N'Precio Unitario', 120, N'System.Decimal', 1, N'{0:C2}', 0, 0, NULL, NULL),
    (N'Total', N'Total', 130, N'System.Decimal', 0, N'{0:C2}', 1, 1, N'Precio', N'Cantidad')
GO
/****** Object:  Table [dbo].[ConciliacionesIaLog]
    Registro (deduplicado por Uuid) de facturas conciliadas con el emparejador
    automático (IA) en /api/ordenescompra/conciliar-factura — base para
    facturar el add-on "Conciliación de factura con IA" a Pilaris. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ConciliacionesIaLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Uuid] [nvarchar](50) NOT NULL,
	[FolioOC] [nvarchar](50) NOT NULL,
	[Fecha] [datetime] NOT NULL,
	[Usuario] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_ConciliacionesIaLog_Uuid] UNIQUE NONCLUSTERED
(
	[Uuid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ConciliacionesIaLog] ADD  DEFAULT (getdate()) FOR [Fecha]
GO
