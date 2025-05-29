using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.DataBase;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Usuarios.Tests.Infrastructure.Tests.DataBase; 
   public class MongoInitializerTests
   {
       [Fact]
       public void Initialize_CreatesExpectedIndexes()
       {
           // Arrange
           var mockMongoClient = new Mock<IMongoClient>();
           var mockDatabase = new Mock<IMongoDatabase>();
           var mockUsuariosCollection = new Mock<IMongoCollection<UsuarioMongo>>();
           var mockActividadesCollection = new Mock<IMongoCollection<ActividadMongo>>();
           var mockUsuarioIndexes = new Mock<IMongoIndexManager<UsuarioMongo>>();
           var mockActividadIndexes = new Mock<IMongoIndexManager<ActividadMongo>>();
   
           // Setup GetDatabase
           mockMongoClient.Setup(c => c.GetDatabase("usuarios_db", null))
                          .Returns(mockDatabase.Object);
   
           // Setup GetCollection<UsuarioMongo>
           mockDatabase.Setup(d => d.GetCollection<UsuarioMongo>("usuarios", null))
                       .Returns(mockUsuariosCollection.Object);
   
           mockUsuariosCollection.SetupGet(c => c.Indexes)
                                 .Returns(mockUsuarioIndexes.Object);
   
           // Setup GetCollection<ActividadMongo>
           mockDatabase.Setup(d => d.GetCollection<ActividadMongo>("actividades", null))
                       .Returns(mockActividadesCollection.Object);
   
           mockActividadesCollection.SetupGet(c => c.Indexes)
                                    .Returns(mockActividadIndexes.Object);
   
           var initializer = new MongoInitializer(mockMongoClient.Object);
   
           // Act
           initializer.Initialize();
   
           // Assert
           mockMongoClient.Verify(c => c.GetDatabase("usuarios_db", null), Times.Once);
           mockDatabase.Verify(d => d.GetCollection<UsuarioMongo>("usuarios", null), Times.Once);
           mockUsuarioIndexes.Verify(i =>
               i.CreateOne(It.IsAny<CreateIndexModel<UsuarioMongo>>(), null, default),
               Times.Once);
   
           mockDatabase.Verify(d => d.GetCollection<ActividadMongo>("actividades", null), Times.Once);
           mockActividadIndexes.Verify(i =>
               i.CreateOne(It.IsAny<CreateIndexModel<ActividadMongo>>(), null, default),
               Times.Once);
       }
   }
   