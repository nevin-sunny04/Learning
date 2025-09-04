package com.learning.JobApp.service;

import com.learning.JobApp.model.JobPost;
import com.learning.JobApp.repo.JobRepo;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
public class JobService
{
    @Autowired
    private JobRepo repo;

    public void addJob(JobPost jobPost)
    {
        repo.addJobs(jobPost);
    }

    public List<JobPost> getAllJobs()
    {
        return repo.getAllJobs();
    }

}
