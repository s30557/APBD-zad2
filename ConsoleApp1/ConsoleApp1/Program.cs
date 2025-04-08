using ConsoleApp1;
using Xunit;

public class Tests
{
    // 1. Simple WHERE filter
    // SQL: SELECT * FROM Emp WHERE Job = 'SALESMAN';
    [Fact]
    public void ShouldReturnAllSalesmen()
    {
        var emps = Database.GetEmps();

        var result = emps.Where(e => e.Job == "SALESMAN").ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal("SALESMAN", e.Job));
    }

    // 2. WHERE + OrderBy
    // SQL: SELECT * FROM Emp WHERE DeptNo = 30 ORDER BY Sal DESC;
    [Fact]
    public void ShouldReturnDept30EmpsOrderedBySalaryDesc()
    {
        var emps = Database.GetEmps();

        var result = emps.Where(e => e.DeptNo == 30).OrderByDescending(e => e.Sal).ToList();

        Assert.Equal(2, result.Count);
        Assert.True(result[0].Sal >= result[1].Sal);
    }

    // 3. Subquery using LINQ (IN clause)
    // SQL: SELECT * FROM Emp WHERE DeptNo IN (SELECT DeptNo FROM Dept WHERE Loc = 'CHICAGO');
    [Fact]
    public void ShouldReturnEmployeesFromChicago()
    {
        var emps = Database.GetEmps();
        var depts = Database.GetDepts();

        var chicagoDeptNos = depts.Where(d => d.Loc == "CHICAGO").Select(d => d.DeptNo).ToList();
        var result = emps.Where(e => chicagoDeptNos.Contains(e.DeptNo)).ToList();

        Assert.All(result, e => Assert.Equal(30, e.DeptNo));
    }

    // 4. SELECT projection
    // SQL: SELECT EName, Sal FROM Emp;
    [Fact]
    public void ShouldSelectNamesAndSalaries()
    {
        var emps = Database.GetEmps();

        var result = emps.Select(e => new { e.EName, e.Sal }).ToList();

        Assert.All(result, r =>
        {
            Assert.False(string.IsNullOrWhiteSpace(r.EName));
            Assert.True(r.Sal > 0);
        });
    }

    // 5. JOIN Emp to Dept
    // SQL: SELECT E.EName, D.DName FROM Emp E JOIN Dept D ON E.DeptNo = D.DeptNo;
    [Fact]
    public void ShouldJoinEmployeesWithDepartments()
    {
        var emps = Database.GetEmps();
        var depts = Database.GetDepts();

        // Łączymy pracowników z działami po DeptNo
        var result = emps.Join(depts, e => e.DeptNo, d => d.DeptNo, (e, d) => new
        {
            e.EName,
            d.DName
        }).ToList();

        Assert.Contains(result, r => r.DName == "SALES" && r.EName == "ALLEN");
    }

    // 6. Group by DeptNo
    // SQL: SELECT DeptNo, COUNT(*) FROM Emp GROUP BY DeptNo;
    [Fact]
    public void ShouldCountEmployeesPerDepartment()
    {
        var emps = Database.GetEmps();

        var result = emps.GroupBy(e => e.DeptNo)
            .Select(g => new { DeptNo = g.Key, Count = g.Count() })
            .ToList();

        Assert.Contains(result, g => g.DeptNo == 30 && g.Count == 2);
    }

    // 7. SelectMany (simulate flattening)
    // SQL: SELECT EName, Comm FROM Emp WHERE Comm IS NOT NULL;
    [Fact]
    public void ShouldReturnEmployeesWithCommission()
    {
        var emps = Database.GetEmps();

        var result = emps.Where(e => e.Comm != null).ToList();

        Assert.All(result, r => Assert.NotNull(r.Comm));
    }

    // 8. Join with Salgrade
    // SQL: SELECT E.EName, S.Grade FROM Emp E JOIN Salgrade S ON E.Sal BETWEEN S.Losal AND S.Hisal;
    [Fact]
    public void ShouldMatchEmployeeToSalaryGrade()
    {
        var emps = Database.GetEmps();
        var grades = Database.GetSalgrades();

        var result = emps.Join(grades, e => e.Sal, s => s.Hisal, (e, s) => new
        {
            e.EName,
            s.Grade
        }).ToList();

        Assert.Contains(result, r => r.EName == "ALLEN" && r.Grade == 3);
    }

    // 9. Aggregation (AVG)
    // SQL: SELECT DeptNo, AVG(Sal) FROM Emp GROUP BY DeptNo;
    [Fact]
    public void ShouldCalculateAverageSalaryPerDept()
    {
        var emps = Database.GetEmps();

        var result = emps.GroupBy(e => e.DeptNo)
            .Select(g => new { g.Key, AvgSal = g.Average(e => e.Sal) })
            .ToList();

        Assert.Contains(result, r => r.Key == 30 && r.AvgSal > 1000);
    }

    // 10. Complex filter with subquery and join
    // SQL: SELECT E.EName FROM Emp E WHERE E.Sal > (SELECT AVG(Sal) FROM Emp WHERE DeptNo = E.DeptNo);
    [Fact]
    public void ShouldReturnEmployeesEarningMoreThanDeptAverage()
    {
        var emps = Database.GetEmps();

        var avgSalaries = emps.GroupBy(e => e.DeptNo)
            .Select(g => new { DeptNo = g.Key, AvgSal = g.Average(e => e.Sal) })
            .ToList();

        var result = emps.Where(e => e.Sal > avgSalaries.First(a => a.DeptNo == e.DeptNo).AvgSal)
            .Select(e => e.EName)
            .ToList();

        Assert.Contains("ALLEN", result);
    }
    
    // 11. MAX salary
    // SQL: SELECT MAX(Sal) FROM Emp;
    [Fact]
    public void ShouldReturnMaxSalary()
    {
        var emps = Database.GetEmps();

        var maxSalary = emps.Max(e => e.Sal); 

        Assert.Equal(5000, maxSalary);
    }

    // 12. MIN salary in department 30
    // SQL: SELECT MIN(Sal) FROM Emp WHERE DeptNo = 30;
    [Fact]
    public void ShouldReturnMinSalaryInDept30()
    {
        var emps = Database.GetEmps();

        var minSalary = emps.Where(e => e.DeptNo == 30).Min(e => e.Sal); 

        Assert.Equal(1250, minSalary);
    }

    // 13. Take first 2 employees ordered by hire date
    // SQL: SELECT * FROM Emp ORDER BY HireDate ASC FETCH FIRST 2 ROWS ONLY;
    [Fact]
    public void ShouldReturnFirstTwoHiredEmployees()
    {
        var emps = Database.GetEmps();

        var firstTwo = emps.OrderBy(e => e.HireDate).Take(2).ToList(); 

        Assert.True(firstTwo[0].HireDate <= firstTwo[1].HireDate);
    }

    // 14. DISTINCT job titles
    // SQL: SELECT DISTINCT Job FROM Emp;
    [Fact]
    public void ShouldReturnDistinctJobTitles()
    {
        var emps = Database.GetEmps();

        var jobs = emps.Select(e => e.Job).Distinct().ToList();

        Assert.Contains("PRESIDENT", jobs); 
        Assert.Contains("SALESMAN", jobs);
    }

    // 15. Employees with managers (NOT NULL Mgr)
    // SQL: SELECT * FROM Emp WHERE Mgr IS NOT NULL;
    [Fact]
    public void ShouldReturnEmployeesWithManagers()
    {
        var emps = Database.GetEmps();

        var withMgr = emps.Where(e => e.Mgr != null).ToList();

        Assert.All(withMgr, e => Assert.NotNull(e.Mgr));
    }

    // 16. All employees earn more than 500
    // SQL: SELECT * FROM Emp WHERE Sal > 500; (simulate all check)
    [Fact]
    public void AllEmployeesShouldEarnMoreThan500()
    {
        var emps = Database.GetEmps();

        var result = emps.All(e => e.Sal > 500);

        Assert.True(result);
    }

    // 17. Any employee with commission over 400
    // SQL: SELECT * FROM Emp WHERE Comm > 400;
    [Fact]
    public void ShouldFindAnyWithCommissionOver400()
    {
        var emps = Database.GetEmps();

        var result = emps.Any(e => e.Comm > 400);

        Assert.True(result);
    }

    // 18. Self-join to get employee-manager pairs
    // SQL: SELECT E1.EName AS Emp, E2.EName AS Manager FROM Emp E1 JOIN Emp E2 ON E1.Mgr = E2.EmpNo;
    [Fact]
    public void ShouldReturnEmployeeManagerPairs()
    {
        var emps = Database.GetEmps();

        var result = emps.Join(emps, e1 => e1.Mgr, e2 => e2.EmpNo, (e1, e2) => new
        {
            Employee = e1.EName,
            Manager = e2.EName
        }).ToList();

        Assert.Contains(result, r => r.Employee == "SMITH" && r.Manager == "FORD");
    }

    // 19. Let clause usage (sal + comm)
    // SQL: SELECT EName, (Sal + COALESCE(Comm, 0)) AS TotalIncome FROM Emp;
    [Fact]
    public void ShouldReturnTotalIncomeIncludingCommission()
    {
        var emps = Database.GetEmps();

        var result = emps.Select(e => new
        {
            e.EName,
            Total = e.Sal + (e.Comm ?? 0) 
        }).ToList();

        Assert.Contains(result, r => r.EName == "ALLEN" && r.Total == 1900);
    }

    // 20. Join all three: Emp → Dept → Salgrade
    // SQL: SELECT E.EName, D.DName, S.Grade FROM Emp E JOIN Dept D ON E.DeptNo = D.DeptNo JOIN Salgrade S ON E.Sal BETWEEN S.Losal AND S.Hisal;
    [Fact]
    public void ShouldJoinEmpDeptSalgrade()
    {
        var emps = Database.GetEmps();
        var depts = Database.GetDepts();
        var grades = Database.GetSalgrades();

        // var result = null; 
        //
        // Assert.Contains(result, r => r.EName == "ALLEN" && r.DName == "SALES" && r.Grade == 3);
    }
    
    
    
    public static void Main(string[] args)
    {
        var emps = Database.GetEmps();

        var result = emps.Select(e => new
        {
            e.EName,
            Total = e.Sal + (e.Comm ?? 0) 
        }).ToList();
        foreach (var r in result)
        {
            Console.WriteLine($"Employee: {r.EName}, Total sal: {r.Total}");
        }
        

    }

}