using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;

namespace TanatosAPI.UseCases {
	public class ModeloCanvasUseCase(IModeloCanvasBcp modeloCanvasBcp) {
		public async Task<List<ModeloCanvas>> ObtenerVigentes(string sub, long idNegocio) {
			return await modeloCanvasBcp.ObtenerPorSubYNegocio(sub, idNegocio, filtrarVigentes: true);
		}

		public async Task<ModeloCanvas> Crear(string sub, long idNegocio, string? sociosClave, string? actividadesClave, string? recursosClave, string? propuestaValor, string? relacionesClientes, string? canales, string? segmentosClientes, string? estructuraCostos, string? fuentesIngresos) {
			sociosClave = string.IsNullOrWhiteSpace(sociosClave) ? null : sociosClave.Trim();
			actividadesClave = string.IsNullOrWhiteSpace(actividadesClave) ? null : actividadesClave.Trim();
			recursosClave = string.IsNullOrWhiteSpace(recursosClave) ? null : recursosClave.Trim();
			propuestaValor = string.IsNullOrWhiteSpace(propuestaValor) ? null : propuestaValor.Trim();
			relacionesClientes = string.IsNullOrWhiteSpace(relacionesClientes) ? null : relacionesClientes.Trim();
			canales = string.IsNullOrWhiteSpace(canales) ? null : canales.Trim();
			segmentosClientes = string.IsNullOrWhiteSpace(segmentosClientes) ? null : segmentosClientes.Trim();
			estructuraCostos = string.IsNullOrWhiteSpace(estructuraCostos) ? null : estructuraCostos.Trim();
			fuentesIngresos = string.IsNullOrWhiteSpace(fuentesIngresos) ? null : fuentesIngresos.Trim();

			return await modeloCanvasBcp.Insertar(
				sub, 
				idNegocio, 
				sociosClave, 
				actividadesClave, 
				recursosClave, 
				propuestaValor, 
				relacionesClientes, 
				canales, 
				segmentosClientes,
				estructuraCostos,
				fuentesIngresos
			);
		}

		public async Task<ModeloCanvas> Actualizar(string sub, long id, string? sociosClave, string? actividadesClave, string? recursosClave, string? propuestaValor, string? relacionesClientes, string? canales, string? segmentosClientes, string? estructuraCostos, string? fuentesIngresos) {
			sociosClave = string.IsNullOrWhiteSpace(sociosClave) ? null : sociosClave.Trim();
			actividadesClave = string.IsNullOrWhiteSpace(actividadesClave) ? null : actividadesClave.Trim();
			recursosClave = string.IsNullOrWhiteSpace(recursosClave) ? null : recursosClave.Trim();
			propuestaValor = string.IsNullOrWhiteSpace(propuestaValor) ? null : propuestaValor.Trim();
			relacionesClientes = string.IsNullOrWhiteSpace(relacionesClientes) ? null : relacionesClientes.Trim();
			canales = string.IsNullOrWhiteSpace(canales) ? null : canales.Trim();
			segmentosClientes = string.IsNullOrWhiteSpace(segmentosClientes) ? null : segmentosClientes.Trim();
			estructuraCostos = string.IsNullOrWhiteSpace(estructuraCostos) ? null : estructuraCostos.Trim();
			fuentesIngresos = string.IsNullOrWhiteSpace(fuentesIngresos) ? null : fuentesIngresos.Trim();

			ModeloCanvas existente = (await modeloCanvasBcp.Obtener(id, validarVigencia: true, validarSub: sub))!;
			
			if (existente.SociosClave != sociosClave || existente.ActividadesClave != actividadesClave || existente.RecursosClave != recursosClave ||
				existente.PropuestaValor != propuestaValor || existente.RelacionesClientes != relacionesClientes || existente.Canales != canales ||
				existente.SegmentosClientes != segmentosClientes || existente.EstructuraCostos != estructuraCostos || existente.FuentesIngresos != fuentesIngresos) {
				
				existente.SociosClave = sociosClave;
				existente.ActividadesClave = actividadesClave;
				existente.RecursosClave = recursosClave;
				existente.PropuestaValor = propuestaValor;
				existente.RelacionesClientes = relacionesClientes;
				existente.Canales = canales;
				existente.SegmentosClientes = segmentosClientes;
				existente.EstructuraCostos = estructuraCostos;
				existente.FuentesIngresos = fuentesIngresos;
				await modeloCanvasBcp.Modificar(existente);
			}

			return existente;
		}

		public async Task Eliminar(string sub, long id) {
			ModeloCanvas? existente = await modeloCanvasBcp.Obtener(id, filtrarVigente: true, validarSub: sub);
			if (existente != null) {
				await modeloCanvasBcp.Eliminar(existente);
			}
		}
	}
}
