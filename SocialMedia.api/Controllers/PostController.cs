using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SocialMedia.api.Responses;
using SocialMedia.core.CutomEntities;
using SocialMedia.core.DTOs;
using SocialMedia.core.Entities;
using SocialMedia.core.Interfaces;
using SocialMedia.core.QueryFIlters;
using SocialMedia.infrastructure.Interfaces;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace SocialMedia.api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IMapper _mapper;
        private readonly IUriService _uriService;
        
        public PostController(IPostService postService, IMapper mapper, IUriService uriService) {
            _postService = postService;
            _mapper = mapper;
            _uriService = uriService;
        }

        /// <summary>
        /// Retrieve all posts
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet(Name = nameof(GetPosts))]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponses<IEnumerable<PostDto>>))] // poniendo el segundo parametro de Type, hacemos que en la documentacion se vea un ejemplo claro de lo que devuelve el metodo cuando se ejecuta correctamente
        [ProducesResponseType((int)HttpStatusCode.BadRequest)] // ESTAS 2 ULTIMAS LINEAS SIRVEN PRINCIPALNENTE PARA LA DOCUMENTACION DE LA API. Indicamos el tipo de dato que nos puede devolver y  
        
        public  IActionResult GetPosts([FromQuery]PostQueryFilter filter) // se pone el [FromQuery] para indicar a la aplicación que coja los valores de la url y cree un objeto PostQueryFilter con esos parametros
        {
            var posts = _postService.GetPosts(filter);
            var postsDto = _mapper.Map<IEnumerable<PostDto>>(posts);
            
            var metadata = new Metadata
            {
                CurrentPage = posts.CurrentPage,
                TotalCount = posts.TotalCount,
                TotalPages = posts.TotalPages,
                PageSize = posts.PageSize,
                HasPreviousPage = posts.HasPreviousPage,
                HasNextPage = posts.HasNextPage,
                NextPageUrl = _uriService.GetPostPaginationUri(filter, Url.RouteUrl(nameof(GetPosts))).ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filter, Url.RouteUrl(nameof(GetPosts))).ToString(),
            };
            var response = new ApiResponses<IEnumerable<PostDto>>(postsDto)
            {
                Metadata = metadata
            };
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
            return Ok(response);
        }
        [HttpGet("post/{id}")]
        public async Task<IActionResult> GetPost(int id)
        {
            var post = await _postService.GetPost(id);
            var postDto = _mapper.Map<PostDto>(post);
            var response = new ApiResponses<PostDto>(postDto);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Post(PostDto postDto)
        {
            var post = _mapper.Map<Post>(postDto);
            await _postService.InsertPost(post);
            var response = new ApiResponses<Post>(post);
            return Ok(response);
        }
        [HttpPut]
        public async Task<IActionResult> Put(int id, PostDto postDto)
        {
            var post = _mapper.Map<Post>(postDto);
            await _postService.UpdatePost(post);
            var response = new ApiResponses<Post>(post);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _postService.DeletePost(id);
            var response = new ApiResponses<bool>(result);
            return Ok(response);
        }
    }
   
}

