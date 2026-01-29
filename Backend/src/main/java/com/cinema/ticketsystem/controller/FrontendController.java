package com.cinema.ticketsystem.controller;

import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.GetMapping;

@Controller
public class FrontendController {

        // Forward non-API routes to Angular so client-side routing works on refresh.
        @GetMapping(value = {
            "/",
            "/{path:^(?!api$)[^\\.]*}",
            "/{path:^(?!api$)[^\\.]*}/**"
        })
    public String forwardToSpa() {
        return "forward:/index.html";
    }
}
