using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.ValueObjects;
using MediatR;

namespace Application.Queries;

public class ObtenerTodasLasActividadesQuery : IRequest<IEnumerable<ActividadMongo>> { }
