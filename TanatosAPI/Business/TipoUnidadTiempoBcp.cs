using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Business {
	public class TipoUnidadTiempoBcp(ITipoUnidadTiempoDao tipoUnidadTiempoDao) : ITipoUnidadTiempoBcp {
        public bool EstaVigente(TipoUnidadTiempo? tipoUnidadTiempo) {
            return tipoUnidadTiempo != null && tipoUnidadTiempo.Vigencia;
        }

        public async Task<List<TipoUnidadTiempo>> ValidarTodosVigentes(HashSet<long> ids, NpgsqlTransaction? transaction = null) {
            if (ids.Count == 0) return [];

            List<TipoUnidadTiempo> vigentes = await ObtenerVigentes(transaction);

            HashSet<long> idsVigentes = [.. vigentes.Select(f => f.Id)];
            foreach (long id in ids) {
                if (!idsVigentes.Contains(id)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, $"La unidad de tiempo con ID {id} no está vigente", "Tipo de unidad de tiempo inválida.");
            }

            return [.. vigentes.Where(f => ids.Contains(f.Id))];
        }

        public async Task<TipoUnidadTiempo?> Obtener(long id, NpgsqlTransaction? transaction = null) {
			return await tipoUnidadTiempoDao.ObtenerPorId(id, transaction);
		}

		public async Task<List<TipoUnidadTiempo>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null) {
			return await tipoUnidadTiempoDao.ObtenerPorVigencia(vigencia, transaction);
		}

		public async Task<List<TipoUnidadTiempo>> ObtenerVigentes(NpgsqlTransaction? transaction = null) {
			return await ObtenerPorVigencia(true, transaction);
		}

		public async Task<TipoUnidadTiempo> Insertar(long id, string nombre, string? nombrePlural, long cantSegundos, long? cantMinutos, long? cantHoras, long? cantDias, bool vigencia, NpgsqlTransaction? transaction = null) {
			TipoUnidadTiempo? existente = await Obtener(id, transaction);
			if (existente != null) throw new ErrorValidacion(TipoErrorValidacion.YaExiste, $"Ya existe una unidad de tiempo con ID {id}.");
			if (nombrePlural == null) throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, $"La unidad de tiempo debe tener un nombre plural.");
			if (cantDias != null && cantHoras == null) throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, $"La unidad de tiempo requiere definir una cantidad de horas que la representan.");
            if (cantHoras != null && cantMinutos == null) throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, $"La unidad de tiempo requiere definir una cantidad de minutos que la representan.");
            
            TipoUnidadTiempo nuevo = new() { 
				Id = id,
				Nombre = nombre,
				NombrePlural = nombrePlural,
				CantSegundos = cantSegundos,
				CantMinutos = cantMinutos,
				CantHoras = cantHoras,
				CantDias = cantDias,
				Vigencia = vigencia
            };
			await tipoUnidadTiempoDao.Insertar(nuevo, transaction);
			return nuevo;
		}

		public async Task<TipoUnidadTiempo> Actualizar(long id, string nombre, string? nombrePlural, long cantSegundos, long? cantMinutos, long? cantHoras, long? cantDias, bool vigencia, NpgsqlTransaction? transaction = null) {
			TipoUnidadTiempo? existente = await Obtener(id, transaction);
			if (existente == null) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, $"No existe una unidad de tiempo con ID {id}.");
			if (nombrePlural == null) throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, $"La unidad de tiempo debe tener un nombre plural.");
			if (cantDias != null && cantHoras == null) throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, $"La unidad de tiempo requiere definir una cantidad de horas que la representan.");
			if (cantHoras != null && cantMinutos == null) throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, $"La unidad de tiempo requiere definir una cantidad de minutos que la representan.");

			if (existente.Nombre != nombre || existente.NombrePlural != nombrePlural || existente.CantSegundos != cantSegundos ||
				existente.CantMinutos != cantMinutos || existente.CantHoras != cantHoras || existente.CantDias != cantDias || 
				existente.Vigencia != vigencia) {
				
				existente.Nombre = nombre;
				existente.NombrePlural = nombrePlural;
				existente.CantSegundos = cantSegundos;
				existente.CantMinutos = cantMinutos;
				existente.CantHoras = cantHoras;
				existente.CantDias = cantDias;
				existente.Vigencia = vigencia;

                await tipoUnidadTiempoDao.Actualizar(existente, transaction);
            }

			return existente;
		}

		public async Task Eliminar(long idTipoUnidadTiempo, NpgsqlTransaction? transaction = null) {
            TipoUnidadTiempo? existente = await Obtener(idTipoUnidadTiempo, transaction);
			if (existente != null) {
				await tipoUnidadTiempoDao.Eliminar(idTipoUnidadTiempo, transaction);
			}
		}
	}
}
