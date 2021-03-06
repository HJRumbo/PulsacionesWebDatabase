using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entity;
using Logica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using pulsacionesdotnet.Models;
using Microsoft.AspNetCore.SignalR;
using pulsacionesdotnet.Hubs;

namespace pulsacionesdotnet.Controllers
{

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PersonaController : ControllerBase
    {
        private readonly IHubContext<SignalHub> _hubContext;
        private readonly PersonaService _personaService;
        public IConfiguration Configuration { get; }
        public PersonaController(IConfiguration configuration, IHubContext<SignalHub> hubContext)
        {
            _hubContext = hubContext;
            Configuration = configuration;
            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];
            _personaService = new PersonaService(connectionString);
        }
        // GET: api/Persona
        [HttpGet]
        public IEnumerable<PersonaViewModel> Gets()
        {
            var personas = _personaService.ConsultarTodos().Select(p=> new PersonaViewModel(p));
            return personas;
        }

        // GET: api/Persona/5
        [HttpGet("{identificacion}")]
        public ActionResult<PersonaViewModel> Get(string identificacion)
        {
            var persona = _personaService.BuscarxIdentificacion(identificacion);
            if (persona == null) return NotFound();
            var personaViewModel = new PersonaViewModel(persona);
            return personaViewModel;
        }
        // POST: api/Persona
        [HttpPost]
        public async Task<ActionResult<PersonaViewModel>> Post(PersonaInputModel personaInput)
        {
            Persona persona = MapearPersona(personaInput);
            var response = _personaService.Guardar(persona);
            if (response.Error) 
            {
                return BadRequest(response.Mensaje);
            }
            var personaViewModel = new PersonaViewModel(response.Persona);
            await _hubContext.Clients.All.SendAsync("PersonaRegistrada", personaViewModel);
            return Ok(personaViewModel);
        }
        // DELETE: api/Persona/5
        [HttpDelete("{identificacion}")]
        public ActionResult<string> Delete(string identificacion)
        {
            string mensaje = _personaService.Eliminar(identificacion);
            return Ok(mensaje);
        }
        private Persona MapearPersona(PersonaInputModel personaInput)
        {
            var persona = new Persona
            {
                Identificacion = personaInput.Identificacion,
                Nombre = personaInput.Nombre,
                Edad = personaInput.Edad,
                Sexo = personaInput.Sexo
            };
            return persona;
        }
        // PUT: api/Persona/5
        [HttpPut("{identificacion}")]
        public ActionResult<string> Put(string identificacion, Persona persona)
        {
            throw new NotImplementedException();
        }
    }
}