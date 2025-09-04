package com.learning.JobApp.controller;

import com.learning.JobApp.model.JobPost;
import com.learning.JobApp.service.JobService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;

import java.util.List;

@Controller
public class JobController
{

    @Autowired
    private JobService jobService;

    @GetMapping({"/", "home"})
    public String home()
    {
        return "home";
    }

    @GetMapping("addjob")
    public String addJob()
    {
        return "addjob";
    }

    @PostMapping("handleForm")
    public String handleForm(JobPost jobPost)
    {
        jobService.addJob(jobPost);
        return "success";
    }

    @GetMapping("viewalljobs")
    public String viewJobs(Model m)
    {
        List<JobPost> jobs = jobService.getAllJobs();
        m.addAttribute("jobPosts", jobs);
        return "viewalljobs";
    }

}
