using NSubstitute;
using NSubstitute.Core.Arguments;
using System;
using System.Collections.Generic;
using System.Text;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TanatosAPI.Test.Business {
	public class CategoriaNormaBcpTest {
		private readonly ICategoriaNormaDao categoriaNormaDao = Substitute.For<ICategoriaNormaDao>();
		private readonly CategoriaNormaBcp categoriaNormaBcp;

		public CategoriaNormaBcpTest() {
			categoriaNormaBcp = new(categoriaNormaDao);
		}

		public static CategoriaNorma CategoriaNormaDummy(
			long id = 1,
			string nombre = "NombreTest",
			string? nombreCorto = "NombreCortoTest",
			string? descripcion = "DescripcionTest",
			bool vigencia = true
		) => new() { 
			Id = id,
			Nombre = nombre,
			NombreCorto = nombreCorto,
			Descripcion = descripcion,
			Vigencia = vigencia
		};

		[Theory]
		[InlineData(1L, 1L)]
		[InlineData(2L, 2L)]
		[InlineData(3L, null)]
		public async Task ObtenerPorIdTest(long id, long? expectedIdResult) {
			categoriaNormaDao.ObtenerPorId(1).Returns(CategoriaNormaDummy(id: 1));
			categoriaNormaDao.ObtenerPorId(2).Returns(CategoriaNormaDummy(id: 2));
			categoriaNormaDao.ObtenerPorId(3).Returns((CategoriaNorma?)null);

			CategoriaNorma? elemento = await categoriaNormaBcp.ObtenerPorId(id);
			Assert.Equal(expectedIdResult, elemento?.Id);
		}

		[Fact]
		public async Task ObtenerVigentesTest() {
			categoriaNormaDao.ObtenerPorVigencia(true).Returns([
				CategoriaNormaDummy(id: 1, vigencia: true),
				CategoriaNormaDummy(id: 2, vigencia: true)
			]);

			List<CategoriaNorma> categorias = await categoriaNormaBcp.ObtenerVigentes();
			Assert.All(categorias, categoria => Assert.True(categoria.Vigencia));
			Assert.Equal(2, categorias.Count);
			await categoriaNormaDao.Received(1).ObtenerPorVigencia(true);
		}

		[Theory]
		[InlineData(true, true, 2)]
		[InlineData(false, false, 1)]
		[InlineData(null, null, 3)]
		public async Task ObtenerPorVigenciaTest(bool? vigencia, bool? expectedVigencia, int expectedCount) {
			categoriaNormaDao.ObtenerPorVigencia(true).Returns([
				CategoriaNormaDummy(id: 1, vigencia: true),
				CategoriaNormaDummy(id: 2, vigencia: true)
			]);
			categoriaNormaDao.ObtenerPorVigencia(false).Returns([
				CategoriaNormaDummy(id: 3, vigencia: false),
			]);
			categoriaNormaDao.ObtenerPorVigencia(null).Returns([
				CategoriaNormaDummy(id: 1, vigencia: true),
				CategoriaNormaDummy(id: 2, vigencia: true),
				CategoriaNormaDummy(id: 3, vigencia: false),
			]);

			List<CategoriaNorma> categorias = await categoriaNormaBcp.ObtenerPorVigencia(vigencia);
			if (expectedVigencia != null) Assert.All(categorias, categoria => Assert.Equal(expectedVigencia, categoria.Vigencia));
			Assert.Equal(expectedCount, categorias.Count);
			await categoriaNormaDao.Received(1).ObtenerPorVigencia(expectedVigencia);
		}

		[Theory]
		[InlineData(1, "Nombre1", "NombreCorto1", "Descripcion1", true)]
		[InlineData(2, "Nombre2", "NombreCorto2", "Descripcion2", false)]
		public async Task RegistrarCategoriaTest(long id, string nombre, string? nombreCorto, string? descripcion, bool vigencia) {
			CategoriaNorma categoria = await categoriaNormaBcp.RegistrarCategoria(id, nombre, nombreCorto, descripcion, vigencia);
			
			Assert.Equal(id, categoria.Id);
			Assert.Equal(nombre, categoria.Nombre);
			Assert.Equal(nombreCorto, categoria.NombreCorto);
			Assert.Equal(descripcion, categoria.Descripcion);
			Assert.Equal(vigencia, categoria.Vigencia);
			await categoriaNormaDao.Received(1).Insertar(Arg.Is<CategoriaNorma>(c => 
				c.Id == id && 
				c.Nombre == nombre && 
				c.NombreCorto == nombreCorto &&
				c.Descripcion == descripcion &&
				c.Vigencia == vigencia
			));
		}

		[Fact]
		public async Task ActualizarCategoriaTest() {
			CategoriaNorma categoria = CategoriaNormaDummy();
			await categoriaNormaBcp.ActualizarCategoria(categoria);
			await categoriaNormaDao.Received(1).Actualizar(Arg.Is<CategoriaNorma>(c =>
				c.Id == categoria.Id &&
				c.Nombre == categoria.Nombre &&
				c.NombreCorto == categoria.NombreCorto &&
				c.Descripcion == categoria.Descripcion &&
				c.Vigencia == categoria.Vigencia
			));
		}

		[Fact]
		public async Task EliminarCategoriaTest() {
			await categoriaNormaBcp.EliminarCategoria(1);
			await categoriaNormaDao.Received(1).Eliminar(Arg.Is<long>(c => c == 1));
		}
	}
}
