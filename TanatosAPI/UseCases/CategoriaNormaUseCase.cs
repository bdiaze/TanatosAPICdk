using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces;
using TanatosAPI.Repositories;

namespace TanatosAPI.UseCases {
	public class CategoriaNormaUseCase(ICategoriaNormaBcp categoriaNormaBcp) {
		public async Task<List<CategoriaNorma>> ObtenerVigentes() {
			return await categoriaNormaBcp.ObtenerVigentes();
		}

		public async Task<List<CategoriaNorma>> ObtenerPorVigencia(bool? vigencia) {
			return await categoriaNormaBcp.ObtenerPorVigencia(vigencia);
		}

		public async Task<CategoriaNorma> RegistrarCategoria(long id, string nombre, string? nombreCorto, string? descripcion, bool vigencia) {
			CategoriaNorma? existente = await categoriaNormaBcp.ObtenerPorId(id);
			if (existente != null) {
				if (existente.Nombre != nombre || existente.NombreCorto != nombreCorto || existente.Descripcion != descripcion || existente.Vigencia != vigencia) {
					throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, $"Ya existe una categoría de norma con ID {id}.");
				} else {
					return existente;
				}
			}

			return await categoriaNormaBcp.RegistrarCategoria(id, nombre, nombreCorto, descripcion, vigencia);
		}

		public async Task<CategoriaNorma> ActualizarCategoria(long id, string nombre, string? nombreCorto, string? descripcion, bool vigencia) {
			CategoriaNorma? existente = await categoriaNormaBcp.ObtenerPorId(id);
			if (existente == null) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, $"No existe la categoría de norma con ID {id}.");
			}

			if (existente.Nombre != nombre || existente.NombreCorto != nombreCorto || existente.Descripcion != descripcion || existente.Vigencia != vigencia) {
				existente.Nombre = nombre;
				existente.NombreCorto = nombreCorto;
				existente.Descripcion = descripcion;
				existente.Vigencia = vigencia;
				await categoriaNormaBcp.ActualizarCategoria(existente);
			}

			return existente;
		}

		public async Task EliminarCategoria(long id) {
			CategoriaNorma? existente = await categoriaNormaBcp.ObtenerPorId(id);
			if (existente != null) {
				await categoriaNormaBcp.EliminarCategoria(id);
			}
		}
	}
}
