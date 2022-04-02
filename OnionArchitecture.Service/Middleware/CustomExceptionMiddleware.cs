﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OnionArchitecture.Service.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OnionArchitecture.Service.Middleware
{
    public class CustomExceptionMiddleware
    {
        //A function that can process an HTTP request.
        private readonly RequestDelegate _next;

        private readonly ILogger<CustomExceptionMiddleware> _logger;
        public CustomExceptionMiddleware(RequestDelegate next, ILogger<CustomExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// The HttpContext for the request.
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <returns>A task that represents the completion of request processing.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                //manage the exceptions
                await HandleExceptionAsync(context, ex, _logger);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger<CustomExceptionMiddleware> logger)
        {
            int code;
            var result = exception.Message;

            switch (exception)
            {
                case ValidationException validationException:
                    code = (int)HttpStatusCode.BadRequest;
                    result = JsonConvert.SerializeObject(validationException.Failures);
                    break;
                case BadRequestException badRequestException:
                    code = (int)HttpStatusCode.BadRequest;
                    result = badRequestException.Message;
                    break;
                case DeleteFailureException deleteFailureException:
                    code = (int)HttpStatusCode.BadRequest;
                    result = deleteFailureException.Message;
                    break;
                case NotFoundException:
                    code = (int)HttpStatusCode.NotFound;
                    break;
                default:
                    code = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            logger.LogError(result);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = code;
            return context.Response.WriteAsync(JsonConvert.SerializeObject(new { StatusCode = code, ErrorMessage = exception.Message }));
        }

    }
}